using AngleSharp.Dom;
using Bunit;
using DemoApp.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Blazorme.Tests;

/// <summary>
/// Tests for the demo pages.
///
/// DemoApp had no coverage at all until a NullReferenceException on Close/Reset turned up during
/// manual verification of the .NET 10 StreamSaver rewrite. It is only a demo, but it is also the
/// only place the libraries are exercised together the way a consuming app uses them, so the
/// wiring is worth pinning.
///
/// Every interaction here is dispatched with the *Async* trigger helpers and awaited. The
/// synchronous ones return once the handler yields, and AppendAsync ends with `await Task.Delay(1)`,
/// so a synchronous Click lets the next interaction race the tail of the previous one. Written that
/// way this suite failed roughly one run in eight.
/// </summary>
public class DemoAppTests : BunitContext
{
    private readonly IStreamSaver _streamSaver = Substitute.For<IStreamSaver>();
    private readonly RecordingStream _stream = new();

    public DemoAppTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _streamSaver.CreateWritableFileStreamAsync(Arg.Any<string>())
            .Returns(_ => Task.FromResult<Stream>(_stream));
        Services.AddSingleton(_streamSaver);
    }

    /// <summary>Captures what the page writes, and whether it was disposed.</summary>
    private sealed class RecordingStream : Stream
    {
        private readonly MemoryStream _written = new();

        public int DisposeCount { get; private set; }
        public string Text => System.Text.Encoding.UTF8.GetString(_written.ToArray());

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _written.Write(buffer, offset, count);
            return Task.CompletedTask;
        }

        public override ValueTask DisposeAsync()
        {
            DisposeCount++;
            return ValueTask.CompletedTask;
        }
    }

    private static IElement Button(IRenderedComponent<StreamSaverDemo> page, string label)
        => page.FindAll("button").Single(b => b.TextContent.Trim() == label);

    private static Task ClickAsync(IRenderedComponent<StreamSaverDemo> page, string label)
        => Button(page, label).ClickAsync(new MouseEventArgs());

    private static Task TypeAsync(IRenderedComponent<StreamSaverDemo> page, string selector, string value)
        => page.Find(selector).ChangeAsync(new ChangeEventArgs { Value = value });

    private const string FileNameBox = "input[placeholder='enter file name']";
    private const string TextBox = "input[placeholder='enter text']";

    [Fact]
    public async Task REGRESSION_Close_before_any_Append_does_not_throw()
    {
        // ResetAsync used to do "await _writableFileStream?.DisposeAsync().AsTask()". The
        // null-conditional binds across the whole chain, so with no stream created the expression
        // is a null Task and awaiting it threw NullReferenceException. CloseAsync delegates to
        // ResetAsync, so a download begun with Append could never be finalised either — the
        // close() call died before reaching the writer.
        var page = Render<StreamSaverDemo>();

        var close = async () => await ClickAsync(page, "Close");

        await close.Should().NotThrowAsync();
    }

    [Fact]
    public async Task REGRESSION_Reset_before_any_Append_does_not_throw()
    {
        var page = Render<StreamSaverDemo>();

        var reset = async () => await ClickAsync(page, "Reset");

        await reset.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Appending_creates_one_stream_and_writes_the_accumulated_text()
    {
        var page = Render<StreamSaverDemo>();

        await TypeAsync(page, FileNameBox, "report.txt");
        await TypeAsync(page, TextBox, "hello");
        await ClickAsync(page, "Add");
        await ClickAsync(page, "Append");

        await _streamSaver.Received(1).CreateWritableFileStreamAsync("report.txt");
        _stream.Text.Should().Be("hello" + Environment.NewLine);
    }

    [Fact]
    public async Task Repeated_appends_reuse_the_same_stream_and_accumulate()
    {
        var page = Render<StreamSaverDemo>();
        var lines = new[] { "one", "two", "three" };

        await TypeAsync(page, FileNameBox, "report.txt");

        foreach (var line in lines)
        {
            await TypeAsync(page, TextBox, line);
            await ClickAsync(page, "Add");
            await ClickAsync(page, "Append");
        }

        // The file is opened once, not once per Append.
        await _streamSaver.Received(1).CreateWritableFileStreamAsync(Arg.Any<string>());
        _stream.Text.Should().Be(string.Concat(lines.Select(l => l + Environment.NewLine)));
    }

    [Fact]
    public async Task Closing_after_appending_disposes_the_stream_exactly_once()
    {
        var page = Render<StreamSaverDemo>();

        await TypeAsync(page, FileNameBox, "report.txt");
        await TypeAsync(page, TextBox, "data");
        await ClickAsync(page, "Add");
        await ClickAsync(page, "Append");

        await ClickAsync(page, "Close");

        // Disposing is what calls close() on the JS writer and finalises the download.
        _stream.DisposeCount.Should().Be(1);

        // A second Close must not dispose it again.
        await ClickAsync(page, "Close");
        _stream.DisposeCount.Should().Be(1);
    }

    [Fact]
    public async Task The_file_name_locks_once_writing_has_started_and_unlocks_on_reset()
    {
        var page = Render<StreamSaverDemo>();

        page.Find(FileNameBox).HasAttribute("disabled").Should().BeFalse();

        await TypeAsync(page, FileNameBox, "report.txt");
        await TypeAsync(page, TextBox, "data");
        await ClickAsync(page, "Add");
        await ClickAsync(page, "Append");

        page.Find(FileNameBox).HasAttribute("disabled").Should().BeTrue("the file is already open");

        await ClickAsync(page, "Reset");

        page.Find(FileNameBox).HasAttribute("disabled").Should().BeFalse("reset starts a new file");
    }

    [Fact]
    public async Task Lorem_ipsum_is_added_the_requested_number_of_times()
    {
        var page = Render<StreamSaverDemo>();

        await TypeAsync(page, FileNameBox, "report.txt");
        await TypeAsync(page, "input[type=number]", "3");
        await ClickAsync(page, "Add lorem ipsum");
        await ClickAsync(page, "Append");

        var paragraphs = _stream.Text.Split(
            Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        paragraphs.Should().HaveCount(3);
        paragraphs.Should().OnlyContain(p => p.StartsWith("Lorem ipsum dolor sit amet"));
        paragraphs.Distinct().Should().ContainSingle("every paragraph is the same constant");
    }
}
