using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;

namespace BlazormeStreamSaver
{
    internal class WritableFileStream : Stream
    {
        //// TODO: INCLUDE IF YOU CAN CONVERT JS FILES TO MODULE
        //private readonly IJSObjectReference _streamSaverModule;
        //private readonly IJSObjectReference _jsInteropModule;
        private readonly IJSRuntime _jsRuntime;
        private readonly string _fileName;

        private JSObject _streamSaverJsObject;
        private JSObject _writeableStreamJsObject;
        private JSObject _writerJsObject;

        public WritableFileStream(IJSRuntime jsRuntime, string fileName)
        {
            _jsRuntime = jsRuntime;
            _fileName = fileName;
        }

        //// TODO: INCLUDE IF YOU CAN CONVERT JS FILES TO MODULE
        //public WritableFileStream(IJSObjectReference streamSaverModule, IJSObjectReference jsInteropModule, 
        //    string fileName)
        //{
        //    _streamSaverModule = streamSaverModule;
        //    _jsInteropModule = jsInteropModule;
        //    _fileName = fileName;
        //}

        public Task CreateAsync()
        {
            var windowJsObject = (JSObject)Runtime.GetGlobalObject("window");
            _streamSaverJsObject = (JSObject)windowJsObject.GetObjectProperty("streamSaver");
            _writeableStreamJsObject =  (JSObject)_streamSaverJsObject.Invoke("createWriteStream", _fileName);
            _writerJsObject = (JSObject)_writeableStreamJsObject.Invoke("getWriter");
            return Task.CompletedTask;
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

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await (Task)_writerJsObject.Invoke("write", Uint8Array.From(buffer));
        }

        public override async ValueTask DisposeAsync()
        {
            await (Task)_writerJsObject.Invoke("close");
            _writerJsObject.Dispose();
            _writeableStreamJsObject.Dispose();
            _streamSaverJsObject.Dispose();
            Close();
            await base.DisposeAsync();
        }
    }
}
