using FluentAssertions;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace Blazorme.Tests;

/// <summary>
/// Characterization tests for Blazorme.Diff. These pin the library's CURRENT observable
/// behaviour, warts included. A red test here means behaviour changed — acceptable only if that
/// was intended.
///
/// Rewritten in 26.9.8, when the diff libraries moved into the package. The calls used to be
/// global — <c>Diff.createTwoFilesPatch</c> and <c>Diff2Html.html</c> — which only worked if the
/// consuming app had added the right CDN script tags itself. They now go through the bundled
/// <c>DiffJsInterop.js</c> module, which loads those libraries on demand.
/// </summary>
public class DiffCharacterizationTests
{
    private const string ModulePath = "./_content/Blazorme.Diff/DiffJsInterop.js";

    private readonly IJSObjectReference _module = Substitute.For<IJSObjectReference>();
    private readonly IJSRuntime _jsRuntime = Substitute.For<IJSRuntime>();

    public DiffCharacterizationTests()
    {
        _jsRuntime.InvokeAsync<IJSObjectReference>("import", Arg.Any<object?[]?>())
            .Returns(new ValueTask<IJSObjectReference>(_module));
        _module.InvokeAsync<string>(Arg.Any<string>(), Arg.Any<object?[]?>())
            .Returns(new ValueTask<string>("<<js>>"));
    }

    /// <summary>The arguments of every call to <paramref name="identifier"/> on the module.</summary>
    private List<object?[]> CallsTo(string identifier) =>
        _module.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IJSObjectReference.InvokeAsync)
                        && Equals(c.GetArguments()[0], identifier))
            .Select(c => (object?[])c.GetArguments().Last()!)
            .ToList();

    [Fact]
    public async Task The_bundled_module_is_imported_from_the_static_web_asset_path()
    {
        await new DiffApi(_jsRuntime).GetAsync("a", "b", "t1", "t2");

        await _jsRuntime.Received().InvokeAsync<IJSObjectReference>(
            "import", Arg.Is<object?[]?>(a => (string)a![0]! == ModulePath));
    }

    [Fact]
    public async Task GetAsync_passes_titles_BEFORE_inputs_to_the_js_side()
    {
        // The argument order is part of the contract with jsdiff's createTwoFilesPatch, and it is
        // not the order the C# parameters are declared in.
        var result = await new DiffApi(_jsRuntime).GetAsync("FIRST-INPUT", "SECOND-INPUT", "t1", "t2");

        result.Should().Be("<<js>>");
        CallsTo("createTwoFilesPatch").Should().ContainSingle()
            .Which.Should().Equal("t1", "t2", "FIRST-INPUT", "SECOND-INPUT");
    }

    [Fact]
    public async Task GetHtmlAsync_Inline_never_touches_js_and_uses_HtmlDiff()
    {
        var html = await new DiffApi(_jsRuntime).GetHtmlAsync(
            "the quick brown fox", "the slow brown fox",
            "t1", "t2", DiffOutputFormat.Inline, DiffStyle.Word);

        await _jsRuntime.DidNotReceiveWithAnyArgs()
            .InvokeAsync<IJSObjectReference>(default!, default(object?[]?));
        html.Should().Contain("<del").And.Contain("<ins");
    }

    [Theory]
    [InlineData(DiffOutputFormat.Row, "line-by-line")]
    [InlineData(DiffOutputFormat.Column, "side-by-side")]
    public async Task GetHtmlAsync_Row_and_Column_map_onto_Diff2Html_output_formats(
        DiffOutputFormat format, string expectedOutputFormat)
    {
        await new DiffApi(_jsRuntime).GetHtmlAsync("a", "b", "t1", "t2", format, DiffStyle.Char);

        CallsTo("createTwoFilesPatch").Should().ContainSingle("the patch is fetched first");

        var config = CallsTo("html").Should().ContainSingle().Subject
            .OfType<BlazormeDiff.HtmlConfiguration>().Single();
        config.OutputFormat.Should().Be(expectedOutputFormat);
        config.DiffStyle.Should().Be("char");
        config.Matching.Should().Be("words");
        config.DrawFileList.Should().BeFalse();
    }

    [Fact]
    public async Task One_import_serves_both_calls_of_a_single_render()
    {
        // Importing per call would create a second reference to the same browser-cached module.
        await new DiffApi(_jsRuntime).GetHtmlAsync(
            "a", "b", "t1", "t2", DiffOutputFormat.Row, DiffStyle.Word);

        await _jsRuntime.Received(1).InvokeAsync<IJSObjectReference>("import", Arg.Any<object?[]?>());
        CallsTo("createTwoFilesPatch").Should().ContainSingle();
        CallsTo("html").Should().ContainSingle();
    }

    [Fact]
    public async Task The_module_reference_is_released_after_use()
    {
        await new DiffApi(_jsRuntime).GetHtmlAsync(
            "a", "b", "t1", "t2", DiffOutputFormat.Row, DiffStyle.Word);

        await _module.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task GetHtmlAsync_returns_empty_for_an_undefined_output_format()
    {
        // The switch has a catch-all arm rather than throwing, so an out-of-range enum silently
        // produces no output at all.
        var html = await new DiffApi(_jsRuntime).GetHtmlAsync(
            "a", "b", "t1", "t2", (DiffOutputFormat)99, DiffStyle.Word);

        html.Should().BeEmpty();
    }

    [Fact]
    public void IDiff_default_titles_are_baked_into_callers_as_First_and_Second()
    {
        DiffInputTitle.First.Should().Be("First");
        DiffInputTitle.Second.Should().Be("Second");
    }
}
