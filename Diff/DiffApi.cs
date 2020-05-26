using Diff;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;

namespace Blazorme
{
    public class DiffApi : IDiff
    {
        private readonly IJSRuntime _jsRuntime;

        public DiffApi(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> GetAsync(string first, string second)
        {
            return await DiffJsInterop.GetAsync(_jsRuntime, first, second);
        }

        public async Task<string> GetHtmlAsync(string first, string second, 
            DiffOutputFormat outputFormat = DiffOutputFormat.Inline)
        {
            if (outputFormat == DiffOutputFormat.Inline)
            {
                return new HtmlDiff.HtmlDiff(first, second).Build();
            }
            else
            {
                return await DiffJsInterop.GetHtmlAsync(_jsRuntime, first, second, outputFormat);
            }
        }

    }
}
