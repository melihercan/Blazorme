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



        private StreamSaverJsObjectRef _streamSaverJsObjectRef;
        private StreamSaverJsObjectRef _writableStreamJsObjectRef;
        private StreamSaverJsObjectRef _writerJsObjectRef;

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

            var windowJsObject = Runtime.GetGlobalObject("window") as JSObject;
            _streamSaverJsObject = windowJsObject.GetObjectProperty("streamSaver") as JSObject;
            _writeableStreamJsObject = _streamSaverJsObject.Invoke("createWriteStream", _fileName) as JSObject;
            _writerJsObject = _writeableStreamJsObject.Invoke("getWriter") as JSObject;

            //_streamSaverJsObjectRef = _jsRuntime.GetJsPropertyObjectRef("window", "streamSaver");
            //_writableStreamJsObjectRef = _jsRuntime.CallJsMethod<StreamSaverJsObjectRef>(
            //    _streamSaverJsObjectRef,
            //    "createWriteStream",
            //    _fileName);
            //_writerJsObjectRef = _jsRuntime.CallJsMethod<StreamSaverJsObjectRef>(
            //    _writableStreamJsObjectRef,
            //    "getWriter");




            ///            _jsRuntime.CallJsMethodVoid(_writerJsObjectRef, "close");
            ////_jsRuntime.CallJsMethodVoid(_writableStreamJsObjectRef, "close");

            ////_jsRuntime.DeleteJsObjectRef(_writerJsObjectRef.StreamSaverJsObjectRefId);
            ////_jsRuntime.DeleteJsObjectRef(_writableStreamJsObjectRef.StreamSaverJsObjectRefId);

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
            try
            {
                Uint8Array uint8Array = Uint8Array.From(buffer);
                ///var utf8Text = Encoding.UTF8.GetString(buffer);
                var task = (Task)_writerJsObject.Invoke("write", /*utf8Text*//*buffer*/uint8Array);
                await task;
                //await _jsRuntime.CallJsMethodVoidAsync(_writerJsObjectRef, "write", buffer);
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }
            
         //   return Task.CompletedTask;
        }

        public override async ValueTask DisposeAsync()
        {
            await (Task)_writerJsObject.Invoke("close");
            //_jsRuntime.DeleteJsObjectRef(_writerJsObjectRef.StreamSaverJsObjectRefId);
            //_jsRuntime.DeleteJsObjectRef(_writableStreamJsObjectRef.StreamSaverJsObjectRefId);
            //_jsRuntime.DeleteJsObjectRef(_streamSaverJsObjectRef.StreamSaverJsObjectRefId);
            Close();
            await base.DisposeAsync();
        }
    }
}
