using Microsoft.JSInterop;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BlazormeStreamSaver
{
    /// <summary>
    /// A write-only <see cref="Stream"/> that forwards each chunk straight to a StreamSaver.js
    /// writer, so a large download never has to be accumulated in memory.
    /// </summary>
    /// <remarks>
    /// Originally built on <c>System.Private.Runtime.InteropServices.JavaScript</c> — the .NET 5/6
    /// preview WASM interop, removed in .NET 7. It now uses ordinary Blazor JS interop against the
    /// <c>StreamSaverJsInterop.js</c> module, which also means it is no longer WASM-only.
    /// </remarks>
    internal class WritableFileStream : Stream
    {
        private readonly IJSObjectReference _module;
        private readonly IJSObjectReference _writer;
        private bool _disposed;

        internal WritableFileStream(IJSObjectReference module, IJSObjectReference writer)
        {
            _module = module;
            _writer = writer;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => 0;

        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            ValidateBufferArguments(buffer, offset, count);

            // Send exactly the requested window, not the whole array. Stream.WriteAsync(
            // ReadOnlyMemory<byte>) hands us a POOLED buffer that is usually larger than the data,
            // so writing buffer.Length here appended trailing garbage to every download.
            var chunk = new byte[count];
            Buffer.BlockCopy(buffer, offset, chunk, 0, count);

            await _module.InvokeVoidAsync("write", cancellationToken, _writer, chunk);
        }

        public override async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;

                // Closing the writer is what finalises the download; skipping it leaves the
                // browser's save dialog hanging.
                await _module.InvokeVoidAsync("close", _writer);
                await _writer.DisposeAsync();
            }

            Close();
            await base.DisposeAsync();
        }
    }
}
