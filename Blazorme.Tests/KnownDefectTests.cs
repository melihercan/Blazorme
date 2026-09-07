using System.Globalization;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Blazorme.Tests;

/// <summary>
/// Tests that assert behaviour which is WRONG, one per outstanding defect.
///
/// These exist so that fixing a defect shows up as a rewritten test in the diff rather than as
/// silence. When a defect is fixed, REWRITE its test to assert the corrected behaviour — never
/// delete it.
///
/// Two of these are marked PERMANENT: they pin warts that cannot be fixed without breaking the
/// public API, which is under an additive-only guarantee. Do not "fix" those.
/// </summary>
public class KnownDefectTests : BunitContext
{
    public KnownDefectTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void DEFECT_Diff_fetches_its_html_twice_on_the_very_first_render()
    {
        // Diff.razor.cs overrides BOTH OnInitializedAsync and OnParametersSetAsync with the same
        // call. Blazor runs both on the first render, so every mount does the work twice — and
        // for the Row/Column formats that is two round trips to JS instead of one.
        var diff = Substitute.For<IDiff>();
        diff.GetHtmlAsync(default!, default!, default!, default!, default, default)
            .ReturnsForAnyArgs(Task.FromResult("<p>diff</p>"));
        Services.AddSingleton(diff);

        Render<Diff>(p => p.Add(c => c.FirstInput, "a").Add(c => c.SecondInput, "b"));

        diff.ReceivedWithAnyArgs(2).GetHtmlAsync(default!, default!, default!, default!, default, default);
    }

    [Fact]
    public void DEFECT_Split_writes_to_its_own_parameter_and_then_goes_stale()
    {
        // Split.OnInitialized assigns to [Parameter] Cursor when the caller left it empty.
        // OnInitialized runs once, so changing Direction later leaves the cursor describing
        // the OLD direction. Writing to one's own parameter is unsupported in Blazor precisely
        // because the framework owns those properties.
        var cut = Render<Split>(p =>
        {
            p.Add(c => c.Direction, SplitDirection.Vertical);
            p.AddChildContent<SplitPane>(pane => pane.AddChildContent("<p>a</p>"));
            p.AddChildContent<SplitPane>(pane => pane.AddChildContent("<p>b</p>"));
        });

        cut.Instance.Cursor.Should().Be("row-resize");

        cut.Render(p => p.Add(c => c.Direction, SplitDirection.Horizontal));

        cut.Instance.Cursor.Should().Be("row-resize",
            "the cursor is stale — it should have become col-resize");
    }

    [Fact]
    public void DEFECT_Split_lowercases_enum_names_with_the_current_culture()
    {
        // Direction.ToString().ToLower() and GutterAlign.ToString().ToLower() are culture
        // sensitive. This is LATENT, not live: no member of SplitDirection or SplitGutterAlign
        // currently contains a capital 'I', which is the letter Turkish maps to 'ı'. Adding one
        // (SplitGutterAlign.Inside, say) would silently emit an option Split.js cannot match.
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

            options.Direction.Should().Be("horizontal", "no current member contains a capital I");

            // The hazard itself, demonstrated on the letter that would trigger it:
            "Inside".ToLower().Should().NotBe("inside", "tr-TR maps 'I' to 'ı', not 'i'");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void DEFECT_SplitPane_outside_a_Split_throws_a_bare_Exception()
    {
        // A bare Exception cannot be caught selectively by callers.
        // InvalidOperationException is the right type here.
        var act = () => Render<SplitPane>(p => p.AddChildContent("<p>orphan</p>"));

        act.Should().Throw<Exception>()
            .Which.Should().Match<Exception>(
                e => e.GetType() == typeof(Exception)
                     && e.Message == "SplitPane should be a child of Split");
    }

    [Fact]
    public async Task DEFECT_Inline_output_silently_ignores_both_titles_and_style()
    {
        // GetHtmlAsync's Inline branch hands off to HtmlDiff, which takes neither the file
        // titles nor a word/char granularity. The parameters are accepted and discarded.
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
