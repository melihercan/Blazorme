using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Diff
{
    internal class DiffJsInterop
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
