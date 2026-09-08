using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazormeSplit
{
    /// <summary>
    /// Calls Split.js through <c>SplitJsInterop.js</c>, which this package ships and which loads
    /// the library on demand.
    /// </summary>
    /// <remarks>
    /// Until 26.9.8 this was a global <c>Split</c> call that only worked if the consuming app had
    /// added a CDN script tag to its own index.html; without it the component failed at runtime
    /// with a JS interop error. Apps that still have that tag keep working: the module skips
    /// loading when the global is already present.
    /// </remarks>
    internal class JsInterop
    {
        private const string ModulePath = "./_content/Blazorme.Split/SplitJsInterop.js";

        internal static async ValueTask InvokeAsync(IJSRuntime jsRuntime, ElementReference[] elements,
            Options options)
        {
            // Split is invoked once per component, on first render, so the module reference is
            // released immediately rather than held for the component's lifetime. The browser
            // caches the module itself, so a later import costs nothing.
            await using var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);

            await module.InvokeVoidAsync("create", elements, options);
        }
    }
}
