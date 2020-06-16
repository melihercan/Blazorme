using BlazormeSplit;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazormeSplit
{
    internal class JsInterop
    {
        internal static async ValueTask<object>InvokeAsync(IJSRuntime jsRuntime, ElementReference[] elements, 
            Options options)
        {
            //return await jsRuntime.InvokeAsync<object>(
            //    "Split",
            //    new object[]
            //    {
            //        elements,
            //        options
            //    });

            return await jsRuntime.InvokeAsync<object>(
                "blazormeSplit.init",
                new object[]
                {
                    elements,
                    options
                });

        }
    }
}
