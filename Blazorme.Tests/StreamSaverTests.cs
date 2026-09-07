using FluentAssertions;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using NSubstitute;
using Xunit;

namespace Blazorme.Tests;

/// <summary>
/// Tests for Blazorme.StreamSaver, which became testable in Phase 1.
///
/// Phase 0 could not characterize this library at all: it targeted net5.0 only and was built on
/// <c>System.Private.Runtime.InteropServices.JavaScript</c>, the .NET 5/6 preview WASM interop that
/// was removed in .NET 7. Its Phase 0 safety net was the public-API baseline alone. Now that the
/// interop goes through IJSObjectReference, the whole path can be driven from a test.
///
/// The public surface is unchanged — PublicApiSurfaceTests proves that independently.
/// </summary>
public class StreamSaverTests
{
    private readonly IJSObjectReference _module = Substitute.For<IJSObjectReference>();
    private readonly IJSObjectReference _writer = Substitute.For<IJSObjectReference>();
    private readonly IJSRuntime _jsRuntime = Substitute.For<IJSRuntime>();

    public StreamSaverTests()
    {
        _jsRuntime.InvokeAsync<IJSObjectReference>("import", Arg.Any<object?[]?>())
            .Returns(new ValueTask<IJSObjectReference>(_module));
        _module.InvokeAsync<IJSObjectReference>("createWriter", Arg.Any<object?[]?>())
            .Returns(new ValueTask<IJSObjectReference>(_writer));
    }

    /// <summary>The args of the single call to <paramref name="identifier"/> on the module.</summary>
    private object?[] VoidCallArgs(string identifier)
    {
        var calls = _module.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IJSObjectReference.InvokeAsync)
                        && Equals(c.GetArguments()[0], identifier))
            .ToList();

        calls.Should().ContainSingle("expected exactly one '{0}' call", identifier);
        return (object?[])calls[0].GetArguments().Last()!;
    }

    [Fact]
    public async Task Imports_the_interop_module_from_the_static_web_asset_path()
    {
        await new StreamSaver(_jsRuntime).CreateWritableFileStreamAsync("report.csv");

        await _jsRuntime.Received().InvokeAsync<IJSObjectReference>(
            "import",
            Arg.Is<object?[]?>(a =>
                (string)a![0]! == "./_content/Blazorme.StreamSaver/StreamSaverJsInterop.js"));
    }

    [Fact]
    public async Task Creates_the_writer_with_the_requested_file_name()
    {
        await new StreamSaver(_jsRuntime).CreateWritableFileStreamAsync("report.csv");

        await _module.Received().InvokeAsync<IJSObjectReference>(
            "createWriter", Arg.Is<object?[]?>(a => (string)a![0]! == "report.csv"));
    }

    [Fact]
    public async Task The_module_is_imported_once_however_many_streams_are_created()
    {
        var streamSaver = new StreamSaver(_jsRuntime);

        await streamSaver.CreateWritableFileStreamAsync("one.txt");
        await streamSaver.CreateWritableFileStreamAsync("two.txt");

        await _jsRuntime.Received(1).InvokeAsync<IJSObjectReference>("import", Arg.Any<object?[]?>());
    }

    [Fact]
    public async Task Writes_only_the_requested_window_of_the_buffer()
    {
        // The regression this replaces: the old implementation sent the WHOLE array via
        // Uint8Array.From(buffer), ignoring offset and count entirely.
        var stream = await new StreamSaver(_jsRuntime).CreateWritableFileStreamAsync("f");
        var buffer = new byte[] { 0, 0, 1, 2, 3, 0, 0 };

        await stream.WriteAsync(buffer, 2, 3, TestContext.Current.CancellationToken);

        var args = VoidCallArgs("write");
        args[0].Should().BeSameAs(_writer);
        args[1].Should().BeOfType<byte[]>().Which.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task Writes_exactly_the_payload_when_given_an_oversized_pooled_buffer()
    {
        // Stream.WriteAsync(ReadOnlyMemory<byte>) rents a pooled array that is usually LARGER than
        // the data and calls WriteAsync(array, 0, length). This is the path DemoApp uses, and the
        // old code appended the pool's trailing zeros to every downloaded file.
        var stream = await new StreamSaver(_jsRuntime).CreateWritableFileStreamAsync("f");
        var payload = new byte[] { 9, 8, 7 };

        await stream.WriteAsync(payload, TestContext.Current.CancellationToken);

        VoidCallArgs("write")[1].Should().BeOfType<byte[]>()
            .Which.Should().Equal(9, 8, 7);
    }

    [Fact]
    public async Task Rejects_a_window_that_runs_off_the_end_of_the_buffer()
    {
        var stream = await new StreamSaver(_jsRuntime).CreateWritableFileStreamAsync("f");

        var act = async () => await stream.WriteAsync(new byte[4], 2, 5, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Disposing_the_stream_closes_the_writer_exactly_once()
    {
        var stream = await new StreamSaver(_jsRuntime).CreateWritableFileStreamAsync("f");

        await stream.DisposeAsync();
        await stream.DisposeAsync();

        VoidCallArgs("close")[0].Should().BeSameAs(_writer);
        await _writer.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task Disposing_the_service_releases_the_module_only_if_it_was_imported()
    {
        var untouched = new StreamSaver(_jsRuntime);
        await untouched.DisposeAsync();
        await _module.DidNotReceive().DisposeAsync();

        var used = new StreamSaver(_jsRuntime);
        await used.CreateWritableFileStreamAsync("f");
        await used.DisposeAsync();
        await _module.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task Is_a_write_only_forward_only_stream()
    {
        var stream = await new StreamSaver(_jsRuntime).CreateWritableFileStreamAsync("f");

        stream.CanWrite.Should().BeTrue();
        stream.CanRead.Should().BeFalse();
        stream.CanSeek.Should().BeFalse();
    }
}
