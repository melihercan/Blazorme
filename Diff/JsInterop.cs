using Blazorme;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazormeDiff
{
    internal class JsInterop
    {
        internal static async ValueTask<string> GetAsync(IJSRuntime jsRuntime, 
            string firstInput, string secondInput,
            string firstTitle, string secondTitle)
        {
            return await jsRuntime.InvokeAsync<string>(
                "Diff.createTwoFilesPatch",
                new object[]
                {
                    firstTitle, secondTitle, firstInput, secondInput
                });
        }

        internal static async ValueTask<string> GetHtmlAsync(IJSRuntime jsRuntime, 
            string firstInput, string secondInput,
            string firstTitle, string secondTitle,
            DiffOutputFormat outputFormat)
        {
            var diff = await GetAsync(jsRuntime, firstInput, secondInput, firstTitle, secondTitle);

            return outputFormat switch
            {
                DiffOutputFormat.Row => await jsRuntime.InvokeAsync<string>(
                    "Diff2Html.html",
                    new object[]
                    {
                        diff,
                        new Diff2HtmlConfiguration
                        {
                            DrawFileList = false,
                            Matching = Diff2HtmlConfiguration.Words,
                            OutputFormat = Diff2HtmlConfiguration.LineByLine
                        }
                    }),

                DiffOutputFormat.Column => await jsRuntime.InvokeAsync<string>(
                    "Diff2Html.html",
                    new object[]
                    {
                        diff,
                        new Diff2HtmlConfiguration
                        {
                            DrawFileList = false,
                            Matching = Diff2HtmlConfiguration.Words,
                            OutputFormat = Diff2HtmlConfiguration.SideBySide
                        }
                    }),

                _ => string.Empty,
            };
        }
    }
}
