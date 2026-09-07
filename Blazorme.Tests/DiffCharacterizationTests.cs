using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace Blazorme.Tests;

/// <summary>
/// Characterization tests for Blazorme.Diff. These pin the library's CURRENT observable
/// behaviour, warts included, so the .NET 10 migration cannot change it silently.
/// A red test here means behaviour changed — acceptable only if that was intended.
/// </summary>
public class DiffCharacterizationTests
{
    private static IJSRuntime StubJsRuntime(string result = "<<js>>")
    {
        var js = Substitute.For<IJSRuntime>();
        js.InvokeAsync<string>(Arg.Any<string>(), Arg.Any<object?[]?>())
          .Returns(new ValueTask<string>(result));
        return js;
    }

    [Fact]
    public async Task GetAsync_passes_titles_BEFORE_inputs_to_the_js_side()
    {
        // The argument order is part of the contract with diff.js's createTwoFilesPatch,
        // and it is not the order the C# parameters are declared in.
        var js = StubJsRuntime("patch");
        object?[]? captured = null;
        js.InvokeAsync<string>(Arg.Any<string>(), Arg.Do<object?[]?>(a => captured = a))
          .Returns(new ValueTask<string>("patch"));

        var result = await new DiffApi(js).GetAsync("FIRST-INPUT", "SECOND-INPUT", "t1", "t2");

        result.Should().Be("patch");
        await js.Received().InvokeAsync<string>("Diff.createTwoFilesPatch", Arg.Any<object?[]?>());
        captured.Should().Equal("t1", "t2", "FIRST-INPUT", "SECOND-INPUT");
    }

    [Fact]
    public async Task GetHtmlAsync_Inline_never_touches_js_and_uses_HtmlDiff()
    {
        var js = StubJsRuntime();

        var html = await new DiffApi(js).GetHtmlAsync(
            "the quick brown fox", "the slow brown fox",
            "t1", "t2", DiffOutputFormat.Inline, DiffStyle.Word);

        await js.DidNotReceiveWithAnyArgs().InvokeAsync<string>(default!, default(object?[]?));
        html.Should().Contain("<del").And.Contain("<ins");
    }

    [Theory]
    [InlineData(DiffOutputFormat.Row, "line-by-line")]
    [InlineData(DiffOutputFormat.Column, "side-by-side")]
    public async Task GetHtmlAsync_Row_and_Column_map_onto_Diff2Html_output_formats(
        DiffOutputFormat format, string expectedOutputFormat)
    {
        var js = StubJsRuntime("html");
        var calls = new List<(string Identifier, object?[]? Args)>();
        js.InvokeAsync<string>(Arg.Do<string>(_ => { }), Arg.Any<object?[]?>())
          .Returns(ci =>
          {
              calls.Add((ci.ArgAt<string>(0), ci.ArgAt<object?[]?>(1)));
              return new ValueTask<string>("html");
          });

        await new DiffApi(js).GetHtmlAsync("a", "b", "t1", "t2", format, DiffStyle.Char);

        calls.Should().HaveCount(2, "the patch is fetched first, then rendered");
        calls[0].Identifier.Should().Be("Diff.createTwoFilesPatch");
        calls[1].Identifier.Should().Be("Diff2Html.html");

        var config = calls[1].Args!.OfType<BlazormeDiff.HtmlConfiguration>().Single();
        config.OutputFormat.Should().Be(expectedOutputFormat);
        config.DiffStyle.Should().Be("char");
        config.Matching.Should().Be("words");
        config.DrawFileList.Should().BeFalse();
    }

    [Fact]
    public async Task GetHtmlAsync_returns_empty_for_an_undefined_output_format()
    {
        // The switch has a catch-all arm rather than throwing, so an out-of-range enum
        // silently produces no output at all.
        var js = StubJsRuntime("html");

        var html = await new DiffApi(js).GetHtmlAsync(
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
