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
            DiffOutputFormat outputFormat, DiffStyle style)
        {
            var diff = await GetAsync(jsRuntime, firstInput, secondInput, firstTitle, secondTitle);

            string styleStr = style == DiffStyle.Word ? HtmlConfiguration.Word : HtmlConfiguration.Char;
            return outputFormat switch
            {
                DiffOutputFormat.Row => await jsRuntime.InvokeAsync<string>(
                    "Diff2Html.html",
                    new object[]
                    {
                        diff,
                        new HtmlConfiguration
                        {
                            DiffStyle = styleStr,
                            DrawFileList = false,
                            Matching = HtmlConfiguration.Words,
                            OutputFormat = HtmlConfiguration.LineByLine
                        }
                    }),

                DiffOutputFormat.Column => await jsRuntime.InvokeAsync<string>(
                    "Diff2Html.html",
                    new object[]
                    {
                        diff,
                        new HtmlConfiguration
                        {
                            DiffStyle = styleStr,
                            DrawFileList = false,
                            Matching = HtmlConfiguration.Words,
                            OutputFormat = HtmlConfiguration.SideBySide
                        }
                    }),

                _ => string.Empty,
            };
        }
    }
}
