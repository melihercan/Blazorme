using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Split
{
    internal class JsInterop
    {
        internal static async ValueTask<object>InvokeAsync(IJSRuntime jsRuntime, string[] elements, Options options)
        {
            return await jsRuntime.InvokeAsync<object>(
                "Split",
                new object[]
                {
                    elements,
                    options
                });
        }
    }
}
