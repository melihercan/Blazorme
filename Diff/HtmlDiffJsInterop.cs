using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Diff
{
    public class HtmlDiffJsInterop
    {
        public static ValueTask<string> Invoke(IJSRuntime jsRuntime, string firstHtml, string secondHtml)
        {
            // Implemented in htmldiff.js
            return jsRuntime.InvokeAsync<string>(
                "htmldiff",
                new object[] {
                    firstHtml,
                    secondHtml});
        }
    }
}
