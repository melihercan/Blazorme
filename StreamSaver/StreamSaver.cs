using BlazormeStreamSaver;
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
        //// TODO: INCLUDE IF YOU CAN CONVERT JS FILES TO MODULE
        ///  JSISOLATION ONLY WORKS WITH MODULES THAT EXPORT FUNCTIONS. 
        ////private readonly Lazy<Task<IJSObjectReference>> _streamSaverModuleTask;
        ////private readonly Lazy<Task<IJSObjectReference>> _polyfillModuleTask;
        ////private readonly Lazy<Task<IJSObjectReference>> _jsInteropModuleTask;


        private readonly IJSRuntime _jsRuntime;

        public StreamSaver(IJSRuntime jsRuntime)
        {
            //// TODO: INCLUDE IF YOU CAN CONVERT JS FILES TO MODULE
            //_streamSaverModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            //   "import", "./_content/Blazorme.StreamSaver/StreamSaver.min.js").AsTask());
            //_polyfillModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            //   "import", "./_content/Blazorme.StreamSaver/polyfill.min.js").AsTask());
            //_jsInteropModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            //   "import", "./_content/Blazorme.StreamSaver/StreamSaverJsInterop.js").AsTask());

            _jsRuntime = jsRuntime;
        }

        public Task<Stream> CreateWritableFileStreamAsync(string fileName)
        {
            //// TODO: INCLUDE IF YOU CAN CONVERT JS FILES TO MODULE
            //return new WritableFileStream(
            //    await _streamSaverModuleTask.Value,
            //    await _jsInteropModuleTask.Value,
            //    fileName);

            return Task.FromResult((Stream)new WritableFileStream(_jsRuntime, fileName));
        }

        public ValueTask DisposeAsync() 
        {
            //// TODO: INCLUDE IF YOU CAN CONVERT JS FILES TO MODULE
            //if (_streamSaverModuleTask.IsValueCreated)
            //{
            //    var streamSaverModule = await _streamSaverModuleTask.Value;
            //    await streamSaverModule.DisposeAsync();
            //}
            //if (_streamSaverModuleTask.IsValueCreated)
            //{
            //    var polyfillModule = await _polyfillModuleTask.Value;
            //    await polyfillModule.DisposeAsync();
            //}
            //if (_jsInteropModuleTask.IsValueCreated)
            //{
            //    var jsInteropModule = await _jsInteropModuleTask.Value;
            //    await jsInteropModule.DisposeAsync();
            //}
            return ValueTask.CompletedTask;
        }
    }
}
