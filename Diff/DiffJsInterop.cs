using Blazorme;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Diff;

namespace Blazorme
{
    internal class DiffJsInterop
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
                            drawFileList = false,
                            matching = Diff2HtmlConfiguration.Words,
                            outputFormat = Diff2HtmlConfiguration.LineByLine
                        }
                    }),

                DiffOutputFormat.Column => await jsRuntime.InvokeAsync<string>(
                    "Diff2Html.html",
                    new object[]
                    {
                        diff,
                        new Diff2HtmlConfiguration
                        {
                            drawFileList = false,
                            matching = Diff2HtmlConfiguration.Words,
                            outputFormat = Diff2HtmlConfiguration.SideBySide
                        }
                    }),

                _ => string.Empty,
            };
        }

    }
}
