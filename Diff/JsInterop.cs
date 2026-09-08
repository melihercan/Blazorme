using Blazorme;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazormeDiff
{
    /// <summary>
    /// Calls the diff libraries through <c>DiffJsInterop.js</c>, which this package ships and
    /// which loads them on demand.
    /// </summary>
    /// <remarks>
    /// Until 26.9.8 these were global calls — <c>Diff.createTwoFilesPatch</c> and
    /// <c>Diff2Html.html</c> — that only worked if the consuming app had added the right CDN
    /// script tags to its own index.html. Missing tags produced a JS interop error at runtime
    /// rather than anything actionable. Apps that still have those tags keep working: the module
    /// skips loading a library whose global is already present.
    /// </remarks>
    internal class JsInterop
    {
        private const string ModulePath = "./_content/Blazorme.Diff/DiffJsInterop.js";

        private static ValueTask<IJSObjectReference> ImportAsync(IJSRuntime jsRuntime) =>
            jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);

        // Titles are passed before inputs, which is the order jsdiff expects and NOT the order
        // this library's own parameters are declared in.
        private static ValueTask<string> CreateTwoFilesPatchAsync(IJSObjectReference module,
            string firstInput, string secondInput, string firstTitle, string secondTitle) =>
            module.InvokeAsync<string>("createTwoFilesPatch",
                firstTitle, secondTitle, firstInput, secondInput);

        internal static async ValueTask<string> GetAsync(IJSRuntime jsRuntime,
            string firstInput, string secondInput,
            string firstTitle, string secondTitle)
        {
            await using var module = await ImportAsync(jsRuntime);
            return await CreateTwoFilesPatchAsync(module, firstInput, secondInput, firstTitle, secondTitle);
        }

        internal static async ValueTask<string> GetHtmlAsync(IJSRuntime jsRuntime,
            string firstInput, string secondInput,
            string firstTitle, string secondTitle,
            DiffOutputFormat outputFormat, DiffStyle style)
        {
            // One import for both calls; importing twice would create a second reference to the
            // same browser-cached module.
            await using var module = await ImportAsync(jsRuntime);

            var diff = await CreateTwoFilesPatchAsync(module, firstInput, secondInput, firstTitle, secondTitle);

            string styleStr = style == DiffStyle.Word ? HtmlConfiguration.Word : HtmlConfiguration.Char;
            return outputFormat switch
            {
                DiffOutputFormat.Row => await module.InvokeAsync<string>(
                    "html",
                    diff,
                    new HtmlConfiguration
                    {
                        DiffStyle = styleStr,
                        DrawFileList = false,
                        Matching = HtmlConfiguration.Words,
                        OutputFormat = HtmlConfiguration.LineByLine
                    }),

                DiffOutputFormat.Column => await module.InvokeAsync<string>(
                    "html",
                    diff,
                    new HtmlConfiguration
                    {
                        DiffStyle = styleStr,
                        DrawFileList = false,
                        Matching = HtmlConfiguration.Words,
                        OutputFormat = HtmlConfiguration.SideBySide
                    }),

                _ => string.Empty,
            };
        }
    }
}
