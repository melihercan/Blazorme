using BlazormeDiff;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Blazorme
{
    public class DiffApi : IDiff
    {
        private readonly IJSRuntime _jsRuntime;

        public DiffApi(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> GetAsync(string firstInput, string secondInput, 
            string firstTitle, string secondTitle)
        {
            return await JsInterop.GetAsync(_jsRuntime, firstInput, secondInput, firstTitle, secondTitle);
        }
             
        public async Task<string> GetHtmlAsync(string firstInput, string secondInput,
            string firstTitle, string secondTitle,
            DiffOutputFormat outputFormat, DiffStyle style)
        {
            if (outputFormat == DiffOutputFormat.Inline)
            {
                return new HtmlDiff.HtmlDiff(firstInput, secondInput).Build();
            }
            else
            {
                return await JsInterop.GetHtmlAsync(_jsRuntime, firstInput, secondInput, firstTitle, secondTitle, 
                    outputFormat, style);
            }
        }
    }
}
