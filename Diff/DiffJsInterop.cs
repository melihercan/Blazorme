using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Diff
{
    internal class DiffJsInterop
    {
        public static async ValueTask<string> Invoke(IJSRuntime jsRuntime, string firstHtml, string secondHtml)
        {
            var stringOne = "string 1 here";
            var stringTwo = "string 2 here";

            string diffString = string.Empty;
            diffString = await jsRuntime.InvokeAsync<string>(
                "Diff.createTwoFilesPatch",
                new object[]
                {
                    "strings", "strings", stringOne, stringTwo
                });

            Console.WriteLine($" DIFF: {diffString}");

            var diffHtml = await jsRuntime.InvokeAsync<string>(
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
                });
            Console.WriteLine($" DIFF: {diffHtml}");
            return diffHtml;

            // Implemented in htmldiff.js
            //return await jsRuntime.InvokeAsync<string>(
            //    "htmldiff",
            //    new object[] 
            //    {
            //        firstHtml,
            //        secondHtml
            //    });



        }

    }
}
