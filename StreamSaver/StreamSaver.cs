using BlazormeStreamSaver;
using Microsoft.JSInterop;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Blazorme
{
    /// <summary>
    /// Creates streams that write directly to a file the browser is downloading, via
    /// StreamSaver.js, so very large files never have to be held in memory.
    /// </summary>
    public class StreamSaver : IStreamSaver, IAsyncDisposable
    {
        // JS isolation needs an ES module, which is why StreamSaverJsInterop.js exists: it wraps
        // the classic StreamSaver.min.js global. The host page still loads StreamSaver.min.js and
        // polyfill.min.js with <script> tags — those are not modules and cannot be imported.
        private readonly Lazy<Task<IJSObjectReference>> _jsInteropModuleTask;

        public StreamSaver(IJSRuntime jsRuntime)
        {
            _jsInteropModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Blazorme.StreamSaver/StreamSaverJsInterop.js").AsTask());
        }

        public async Task<Stream> CreateWritableFileStreamAsync(string fileName)
        {
            var module = await _jsInteropModuleTask.Value;
            var writer = await module.InvokeAsync<IJSObjectReference>("createWriter", fileName);
            return new WritableFileStream(module, writer);
        }

        public async ValueTask DisposeAsync()
        {
            if (_jsInteropModuleTask.IsValueCreated)
            {
                var module = await _jsInteropModuleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}
