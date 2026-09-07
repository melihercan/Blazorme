using System.Globalization;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Blazorme.Tests;

/// <summary>
/// One test per defect, rewritten in place as each is fixed so the change shows up in the diff
/// rather than as silence.
///
/// Phase 0 pinned seven defects here. Phase 4 fixed four; those now assert the corrected
/// behaviour and are prefixed FIXED_, naming the pin they replace.
///
/// The rest are not going to be fixed, and say why:
/// - LIMITATION_ — inherent to a dependency, not a bug in this code.
/// - PERMANENT_  — fixing it would break the public API, which is under an additive-only
///                 guarantee. Do not "fix" those.
/// </summary>
public class KnownDefectTests : BunitContext
{
    public KnownDefectTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    private BlazormeSplit.Options SingleSplitCallOptions()
        => (BlazormeSplit.Options)JSInterop.Invocations
            .Single(i => i.Identifier == "Split").Arguments[1]!;

    [Fact]
    public void FIXED_Diff_fetches_its_html_once_per_parameter_set()
    {
        // Was: DEFECT_Diff_fetches_its_html_twice_on_the_very_first_render, which asserted 2.
        // Diff.razor.cs used to override BOTH OnInitializedAsync and OnParametersSetAsync with
        // the same call, and Blazor runs both on the first render. The OnInitializedAsync
        // override is gone; OnParametersSetAsync alone covers the first render and every later
        // parameter change.
        var diff = Substitute.For<IDiff>();
        diff.GetHtmlAsync(default!, default!, default!, default!, default, default)
            .ReturnsForAnyArgs(Task.FromResult("<p>diff</p>"));
        Services.AddSingleton(diff);

        var cut = Render<Diff>(p => p.Add(c => c.FirstInput, "a").Add(c => c.SecondInput, "b"));

        diff.ReceivedWithAnyArgs(1).GetHtmlAsync(default!, default!, default!, default!, default, default);

        // Still refreshes when the inputs actually change.
        cut.Render(p => p.Add(c => c.SecondInput, "c"));

        diff.ReceivedWithAnyArgs(2).GetHtmlAsync(default!, default!, default!, default!, default, default);
    }

    [Fact]
    public void FIXED_Split_leaves_its_Cursor_parameter_alone()
    {
        // Was: DEFECT_Split_writes_to_its_own_parameter_and_then_goes_stale.
        // Split.OnInitialized used to assign to [Parameter] Cursor when the caller left it empty.
        // OnInitialized runs once, so changing Direction afterwards left the cursor describing
        // the OLD direction. The cursor is now derived at the point of use, and the parameter is
        // never written — Blazor owns parameter properties.
        var cut = Render<Split>(p =>
        {
            p.Add(c => c.Direction, SplitDirection.Vertical);
            p.AddChildContent<SplitPane>(pane => pane.AddChildContent("<p>a</p>"));
            p.AddChildContent<SplitPane>(pane => pane.AddChildContent("<p>b</p>"));
        });

        cut.Instance.Cursor.Should().BeEmpty("the component must not write to its own parameter");

        cut.Render(p => p.Add(c => c.Direction, SplitDirection.Horizontal));

        cut.Instance.Cursor.Should().BeEmpty();

        // The derived value still reaches JS. Both directions are covered by
        // SplitCharacterizationTests.Derives_a_cursor_from_the_direction_when_none_is_given.
        SingleSplitCallOptions().Cursor.Should().Be("row-resize");
    }

    [Fact]
    public void An_explicit_Cursor_still_wins_over_the_derived_one()
    {
        Render<Split>(p =>
        {
            p.Add(c => c.Cursor, "grabbing");
            p.AddChildContent<SplitPane>(pane => pane.AddChildContent("<p>a</p>"));
            p.AddChildContent<SplitPane>(pane => pane.AddChildContent("<p>b</p>"));
        });

        SingleSplitCallOptions().Cursor.Should().Be("grabbing");
    }

    [Fact]
    public void FIXED_Split_lowercases_enum_names_invariantly()
    {
        // Was: DEFECT_Split_lowercases_enum_names_with_the_current_culture.
        // ToLower() is culture sensitive, and Turkish maps 'I' to 'ı'. The defect was LATENT —
        // no member of SplitDirection or SplitGutterAlign contains a capital 'I' — but adding
        // one (SplitGutterAlign.Inside, say) would have emitted an option Split.js cannot match.
        // Both call sites now use ToLowerInvariant.
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            Render<Split>(p =>
            {
                p.Add(c => c.Direction, SplitDirection.Horizontal);
                p.AddChildContent<SplitPane>(pane => pane.AddChildContent("<p>a</p>"));
                p.AddChildContent<SplitPane>(pane => pane.AddChildContent("<p>b</p>"));
            });

            var options = (BlazormeSplit.Options)JSInterop.Invocations
                .Single(i => i.Identifier == "Split").Arguments[1]!;

            options.Direction.Should().Be("horizontal");

            // The hazard that is now defused, shown on the letter that would have triggered it:
            "Inside".ToLower().Should().NotBe("inside", "tr-TR maps 'I' to 'ı', not 'i'");
            "Inside".ToLowerInvariant().Should().Be("inside", "which is why the code uses this");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void FIXED_SplitPane_outside_a_Split_throws_InvalidOperationException()
    {
        // Was: DEFECT_SplitPane_outside_a_Split_throws_a_bare_Exception. A bare Exception cannot
        // be caught selectively. Narrowing the thrown type to a subclass is not a breaking
        // change: existing catch(Exception) handlers still match it.
        var act = () => Render<SplitPane>(p => p.AddChildContent("<p>orphan</p>"));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("SplitPane should be a child of Split");
    }

    [Fact]
    public async Task LIMITATION_Inline_output_ignores_both_titles_and_style()
    {
        // NOT a defect in this code, and not scheduled for a fix: GetHtmlAsync's Inline branch
        // hands off to htmldiff.net, which has no concept of file titles or word/char
        // granularity. The parameters are accepted and discarded. Rejecting them instead would
        // break callers who pass them harmlessly today.
        var js = Substitute.For<Microsoft.JSInterop.IJSRuntime>();
        var api = new DiffApi(js);

        var word = await api.GetHtmlAsync("a b c", "a x c", "T1", "T2", DiffOutputFormat.Inline, DiffStyle.Word);
        var chr = await api.GetHtmlAsync("a b c", "a x c", "OTHER", "TITLES", DiffOutputFormat.Inline, DiffStyle.Char);

        chr.Should().Be(word);
        word.Should().NotContain("T1").And.NotContain("T2");
    }

    [Fact]
    public void PERMANENT_the_FFmpeg_service_extension_type_name_is_misspelled()
    {
        // "FFmpegerviceExtensions" — the S of "Service" was eaten. It is a public type, so under
        // the additive-only guarantee it CANNOT be renamed. A correctly named type may be added
        // beside it; this one stays.
        //
        // Asserted through the metadata baseline rather than typeof(): Blazorme.FFmpeg targets
        // net5.0 only and cannot be referenced from a net10.0 test project at all.
        var baseline = File.ReadAllText(
            Path.Combine(TestAssemblies.RepositoryRoot, "Blazorme.Tests", "PublicApi.approved.txt"));

        baseline.Should().Contain("type Blazorme.FFmpegerviceExtensions : static class");
    }

    [Fact]
    public void PERMANENT_the_generated_Imports_classes_leak_into_the_public_surface()
    {
        // Each _Imports.razor compiles to a public class. They are meaningless to consumers but
        // they are public types in shipped assemblies, so removing them would be a breaking
        // change. Setting <ExcludeFromCodeCoverage>/internal on generated Razor types is not
        // configurable, so these stay in the API baseline.
        var imports = typeof(Diff).Assembly.GetType("BlazormeDiff._Imports");

        imports.Should().NotBeNull();
        imports!.IsPublic.Should().BeTrue();
    }
}
