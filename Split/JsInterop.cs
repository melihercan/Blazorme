using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Split
{
    internal class JsInterop
    {
        public static ValueTask<string> Prompt(IJSRuntime jsRuntime, string message)
        {
            return new ValueTask<string>();
        }
    }
}
