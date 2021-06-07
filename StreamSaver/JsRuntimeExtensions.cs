using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazormeStreamSaver
{
    public static class JsRuntimeExtensions
    {
        public static StreamSaverJsObjectRef CreateJsObject(this IJSRuntime jsRuntime, object parent, 
            string interface_, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                interface_
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            var jsObjectRef = jsRuntime.Invoke<StreamSaverJsObjectRef>("StreamSaverJsInterop.createObject", invokeParams);
            return jsObjectRef;
        }

        public static void DeleteJsObjectRef(this IJSRuntime jsRuntime,  int id)
        {
            jsRuntime.Invoke<object>(
                "StreamSaverJsInterop.deleteObjectRef",
                new object[]
                {
                    id,
                });
        }

        public static StreamSaverJsObjectRef GetJsPropertyObjectRef(this IJSRuntime jsRuntime, 
            object parent,  string property)
        {
            var jsObjectRef = jsRuntime.Invoke<StreamSaverJsObjectRef>(
                "StreamSaverJsInterop.getPropertyObjectRef",
                new object[]
                {
                    parent,
                    property//,
                    //null
                });
            return jsObjectRef;
        }

        public static ValueTask<StreamSaverJsObjectRef> GetJsPropertyObjectRefAsync(this IJSRuntime jsRuntime,
            object parent, string property)
        {
            var jsObjectRef = jsRuntime.InvokeAsync<StreamSaverJsObjectRef>(
                "StreamSaverJsInterop.getPropertyObjectRef",
                new object[]
                {
                    parent,
                    property
                });//.ConfigureAwait(false);
            return jsObjectRef;
        }


        public static T GetJsPropertyValue<T>(this IJSRuntime jsRuntime, object parent,
            string property, object valueSpec = null)
        {
            var content = jsRuntime.Invoke<T>(
                "StreamSaverJsInterop.getPropertyValue",
                new object[]
                {
                    parent,
                    property,
                    valueSpec
                });
            return content;
        }

        public static IEnumerable<StreamSaverJsObjectRef> GetJsPropertyArray(this IJSRuntime jsRuntime,
            object parent, string property = null)
        {
            var jsObjectRefs = jsRuntime.Invoke<IEnumerable<StreamSaverJsObjectRef>>(
                "StreamSaverJsInterop.getPropertyArray",
                new object[]
                {
                    parent,
                    property,
                });
            return jsObjectRefs;
        }


        public static void SetJsProperty(this IJSRuntime jsRuntime, object parent,
            string property, object value)
        {
            jsRuntime.InvokeVoid(
                "StreamSaverJsInterop.setProperty",
                new object[]
                {
                    parent,
                    property,
                    value
                });
        }

        public static void CallJsMethodVoid(this IJSRuntime jsRuntime, object parent,
            string method, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            jsRuntime.InvokeVoid("StreamSaverJsInterop.callMethod", invokeParams);
        }

        public static T CallJsMethod<T>(this IJSRuntime jsRuntime, object parent,
            string method, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            var ret = jsRuntime.Invoke<T>("StreamSaverJsInterop.callMethod", invokeParams);
            return ret;
        }

        public static ValueTask CallJsMethodVoidAsync(this IJSRuntime jsRuntime, object parent,
            string method, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            return jsRuntime.InvokeVoidAsync("StreamSaverJsInterop.callMethodAsync", invokeParams);
                //.ConfigureAwait(false);
        }

        public static async ValueTask<T> CallJsMethodAsync<T>(this IJSRuntime jsRuntime, 
            object parent, string method, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method
            };
            if(args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            var ret = await jsRuntime.InvokeAsync<T>("StreamSaverJsInterop.callMethodAsync", invokeParams)
                .ConfigureAwait(false);
            return ret;
        }


        public static TValue Invoke<TValue>(this IJSRuntime jsRuntime, string identifier, params object[] args)
        {
            var isWasm = jsRuntime is IJSInProcessRuntime;

            if (isWasm)
            {
                return ((IJSInProcessRuntime)jsRuntime).Invoke<TValue>(identifier, args);
            }
            else
            {
                // Sync call to JSInterop is not possible.
                // Blocking current thread with any kind of Wait throws:
                //   Exception thrown: 'System.Threading.Tasks.TaskCanceledException' in System.Private.CoreLib.dll
                // Async API is required.
                throw new NotImplementedException();
            }
        }

        //private static async void Sync<TValue>(IJSRuntime jsRuntime, string identifier, params object[] args)
        //{
        //    TValue value = default(TValue);
        //    value = await jsRuntime.InvokeAsync<TValue>(identifier, args).Wait();
        //}

        public static void InvokeVoid(this IJSRuntime jsRuntime, string identifier, params object[] args)
        {
            var isWasm = jsRuntime is IJSInProcessRuntime;

            if (isWasm)
            {
                _ = (IJSInProcessRuntime)jsRuntime.Invoke<object>(identifier, args);
            }
            else
            {
                // Sync call to JSInterop is not possible.
                // Blocking current thread with any kind of Wait throws:
                //   Exception thrown: 'System.Threading.Tasks.TaskCanceledException' in System.Private.CoreLib.dll
                // Async API is required.
                throw new NotImplementedException();
            }
        }
    }
}