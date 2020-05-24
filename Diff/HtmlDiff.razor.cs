using Diff;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Blazorme
{
    public partial class HtmlDiff : ComponentBase
    {
        [Inject]
        private IJSRuntime _jsRuntime { get; set; }

        [Parameter]
        public string FirstHtml { get; set; }

        [Parameter]
        public string SecondHtml { get; set; }

        private string _diff { get; set; } = string.Empty;


        protected override async Task OnInitializedAsync()
        {
            _diff = await HtmlDiffJsInterop.Invoke(_jsRuntime, FirstHtml, SecondHtml);

            await base.OnInitializedAsync();
        }

    }
}
