using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazorme
{
    public class StreamSaver : IStreamSaver, IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> streamSaverModuleTask;
        private readonly Lazy<Task<IJSObjectReference>> polyfillModuleTask;
        private readonly Lazy<Task<IJSObjectReference>> jsInteropModuleTask;

        public StreamSaver(IJSRuntime jsRuntime)
        {
            streamSaverModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/StreamSaver/StreamSaver.min.js").AsTask());
            polyfillModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/StreamSaver/polyfill.min.js").AsTask());
            jsInteropModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/StreamSaver/StreamSaverJsInterop.js").AsTask());
        }

        public async Task CopyToAsync(Stream stream)
        {
            var module = await streamSaverModuleTask.Value;
            await module.InvokeAsync<string>("showPrompt", stream);
        }

        public async ValueTask DisposeAsync()
        {
            if (streamSaverModuleTask.IsValueCreated)
            {
                var streamSaverModule = await streamSaverModuleTask.Value;
                await streamSaverModule.DisposeAsync();
            }
            if (streamSaverModuleTask.IsValueCreated)
            {
                var polyfillModule = await polyfillModuleTask.Value;
                await polyfillModule.DisposeAsync();
            }
            if (jsInteropModuleTask.IsValueCreated)
            {
                var jsInteropModule = await jsInteropModuleTask.Value;
                await jsInteropModule.DisposeAsync();
            }
        }
    }
}
