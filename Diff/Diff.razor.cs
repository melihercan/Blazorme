using Diff;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Blazorme
{

    public partial class Diff : ComponentBase
    {
        private string _diff { get; set; } = string.Empty;

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Inject]
        private IDiff DiffApi { get; set; }

        [Parameter]
        public string FirstInput { get; set; } = string.Empty;

        [Parameter]
        public string SecondInput { get; set; } = string.Empty;

        [Parameter]
        public string FirstTitle { get; set; } = DiffInputTitle.First;

        [Parameter]
        public string SecondTitle { get; set; } = DiffInputTitle.Second;

        [Parameter]
        public DiffOutputFormat OutputFormat { get; set; } = DiffOutputFormat.Inline;

        protected override async Task OnInitializedAsync()
        {
            _diff = await DiffApi.GetHtmlAsync(FirstInput, SecondInput, FirstTitle, SecondTitle, OutputFormat);

            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            _diff = await DiffApi.GetHtmlAsync(FirstInput, SecondInput, FirstTitle, SecondTitle, OutputFormat);

            await base.OnParametersSetAsync();
        }
    }
}



