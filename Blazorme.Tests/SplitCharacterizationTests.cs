using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Blazorme.Tests;

/// <summary>
/// Characterization tests for Blazorme.Split. These pin CURRENT behaviour, defects included.
/// </summary>
public class SplitCharacterizationTests : BunitContext
{
    // Rewritten in 26.9.8: Split.js now ships inside the package and is loaded by the bundled
    // SplitJsInterop.js module, so the call is no longer a global "Split" invocation.
    private const string ModulePath = "./_content/Blazorme.Split/SplitJsInterop.js";

    private readonly BunitJSModuleInterop _module;

    public SplitCharacterizationTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _module = JSInterop.SetupModule(ModulePath);
    }

    private IRenderedComponent<Split> RenderSplit(Action<ComponentParameterCollectionBuilder<Split>>? configure = null)
        => Render<Split>(p =>
        {
            configure?.Invoke(p);
            p.AddChildContent<SplitPane>(pane => pane.AddChildContent("<p>left</p>"));
            p.AddChildContent<SplitPane>(pane => pane.AddChildContent("<p>right</p>"));
        });

    private BlazormeSplit.Options SingleSplitCallOptions()
    {
        var call = _module.Invocations["create"].Single();
        return (BlazormeSplit.Options)call.Arguments[1]!;
    }

    [Fact]
    public void Renders_a_container_whose_data_direction_is_the_enum_name_not_lowercased()
    {
        // The CSS selectors in Split.razor are [data-direction=Horizontal] / [data-direction=Vertical],
        // so the markup casing and the stylesheet are coupled. The JS options use lowercase instead.
        var cut = RenderSplit();

        cut.Find("div.blazorme-split").GetAttribute("data-direction").Should().Be("Horizontal");
    }

    [Fact]
    public void Emits_its_stylesheet_inline_in_every_instance()
    {
        var cut = RenderSplit();

        cut.Markup.Should().Contain("<style>").And.Contain(".blazorme-split");
        cut.Markup.Should().Contain("left").And.Contain("right");
    }

    [Fact]
    public void Calls_the_bundled_module_once_on_first_render_with_elements_then_options()
    {
        RenderSplit();

        var call = _module.Invocations["create"].Should().ContainSingle().Subject;
        call.Arguments.Should().HaveCount(2);
        call.Arguments[0].Should().BeOfType<ElementReference[]>().Which.Should().HaveCount(2);
        call.Arguments[1].Should().BeOfType<BlazormeSplit.Options>();
    }

    [Fact]
    public void Imports_the_bundled_module_from_the_static_web_asset_path()
    {
        // The app no longer has to add a split.js script tag of its own; a missing tag used to
        // fail at runtime with a JS interop error.
        RenderSplit();

        _module.Invocations["create"].Should().ContainSingle();
    }

    [Fact]
    public void Lowercases_direction_and_gutter_align_for_the_js_side()
    {
        RenderSplit(p => p
            .Add(c => c.Direction, SplitDirection.Vertical)
            .Add(c => c.GutterAlign, SplitGutterAlign.Start));

        var options = SingleSplitCallOptions();
        options.Direction.Should().Be("vertical");
        options.GutterAlign.Should().Be("start");
    }

    [Fact]
    public void Sends_null_Sizes_when_no_pane_declares_a_size()
    {
        RenderSplit();

        SingleSplitCallOptions().Sizes.Should().BeNull(
            "Split.js treats a null sizes array as 'distribute evenly'");
    }

    [Fact]
    public void Sends_explicit_Sizes_as_soon_as_any_pane_declares_one()
    {
        Render<Split>(p =>
        {
            p.AddChildContent<SplitPane>(pane => pane
                .Add(c => c.SizeInPercentage, 70)
                .AddChildContent("<p>left</p>"));
            p.AddChildContent<SplitPane>(pane => pane.AddChildContent("<p>right</p>"));
        });

        // Note the second pane contributes 0, not 30: the array is taken verbatim.
        SingleSplitCallOptions().Sizes.Should().Equal(70, 0);
    }

    [Fact]
    public void Falls_back_to_DefaultMinSize_for_panes_that_do_not_set_MinSize()
    {
        Render<Split>(p =>
        {
            p.Add(c => c.DefaultMinSize, 42);
            p.AddChildContent<SplitPane>(pane => pane
                .Add(c => c.MinSize, 5)
                .AddChildContent("<p>left</p>"));
            p.AddChildContent<SplitPane>(pane => pane.AddChildContent("<p>right</p>"));
        });

        SingleSplitCallOptions().MinSize.Should().Equal(5, 42);
    }

    [Theory]
    [InlineData(SplitDirection.Horizontal, "col-resize")]
    [InlineData(SplitDirection.Vertical, "row-resize")]
    public void Derives_a_cursor_from_the_direction_when_none_is_given(
        SplitDirection direction, string expectedCursor)
    {
        RenderSplit(p => p.Add(c => c.Direction, direction));

        SingleSplitCallOptions().Cursor.Should().Be(expectedCursor);
    }

    [Fact]
    public void Passes_the_remaining_options_through_unchanged()
    {
        RenderSplit(p => p
            .Add(c => c.ExpandToMin, true)
            .Add(c => c.GutterSize, 7)
            .Add(c => c.SnapOffset, 13)
            .Add(c => c.DragInterval, 3));

        var options = SingleSplitCallOptions();
        options.ExpandToMin.Should().BeTrue();
        options.GutterSize.Should().Be(7);
        options.SnapOffset.Should().Be(13);
        options.DragInterval.Should().Be(3);
    }
}
