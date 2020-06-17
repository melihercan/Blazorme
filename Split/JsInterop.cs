using BlazormeSplit;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazormeSplit
{
    internal class JsInterop
    {
        internal static async ValueTask InvokeAsync(IJSRuntime jsRuntime, ElementReference[] elements, 
            Options options)
        {
            await jsRuntime.InvokeAsync<object>(
                "Split",
                new object[]
                {
                    elements,
                    options
                });

        }
    }
}
