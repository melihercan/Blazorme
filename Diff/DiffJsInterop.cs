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
        internal static async ValueTask<string> Invoke(IJSRuntime jsRuntime, string first, string second, 
            Diff.EOutputFormat outputFormat, string outputTitle)
        {
            var diffString = await jsRuntime.InvokeAsync<string>(
                "Diff.createTwoFilesPatch",
                new object[]
                {
                    outputTitle, outputTitle, first, second
                });

            return outputFormat switch
            {
                Diff.EOutputFormat.LineByLine => await jsRuntime.InvokeAsync<string>(
                    "Diff2Html.html",
                    new object[]
                    {
                        diffString,
                        new Diff2HtmlConfiguration
                        {
                            drawFileList = false,
                            matching = Diff2HtmlConfiguration.Words,
                            outputFormat = Diff2HtmlConfiguration.LineByLine
                        }
                    }),

                Diff.EOutputFormat.SideBySide => await jsRuntime.InvokeAsync<string>(
                    "Diff2Html.html",
                    new object[]
                    {
                        diffString,
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

        internal static async ValueTask<string> Get(IJSRuntime jsRuntime, string first, string second)
        {
            return await jsRuntime.InvokeAsync<string>(
                "Diff.createTwoFilesPatch",
                new object[]
                {
                    "", "", first, second
                });

        }
    }
}
