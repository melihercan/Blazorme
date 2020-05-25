using Diff;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Blazorme
{
    public class DiffApi : IDiffApi
    {
        private readonly IJSRuntime _jsRuntime;

        public DiffApi(IJSRuntime jSRuntime)
        {
            _jsRuntime = jSRuntime;
        }

        public async ValueTask<string> Get(string first, string second)
        {
            return await DiffJsInterop.Get(_jsRuntime, first, second);
        }

        public string GetHtml(string first, string second)
        {
            return null;
        }

    }
}
