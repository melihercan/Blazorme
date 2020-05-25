using Diff;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Blazorme
{

    public partial class Diff : ComponentBase//, IDiff
    {
        private string _diff { get; set; } = string.Empty;

        [Inject]
        private IJSRuntime _jsRuntime { get; set; }

        [Parameter]
        public string FirstString { get; set; } = string.Empty;

        [Parameter]
        public string SecondString { get; set; } = string.Empty;

        public enum EOutputFormat
        {
            HtmlInline,
            TextRow,
            TextColumn,
        }

        [Parameter]
        public EOutputFormat OutputFormat { get; set; } = EOutputFormat.HtmlInline;

        [Parameter]
        public string OutputTitle { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            _diff = await DoDiffAsync(FirstString, SecondString, OutputFormat, OutputTitle);

            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            _diff = await DoDiffAsync(FirstString, SecondString, OutputFormat, OutputTitle);

            await base.OnParametersSetAsync();
        }

        private async Task<string> DoDiffAsync(string first, string second, 
            Diff.EOutputFormat outputformat = Diff.EOutputFormat.HtmlInline, string outputTitle = "")
        {
            if(outputformat == EOutputFormat.HtmlInline)
            {
                return new HtmlDiff.HtmlDiff(first, second).Build();
            }
            else
            {
                return await DiffJsInterop.Invoke(_jsRuntime, first, second, outputformat, outputTitle);
            }
        }
    }
}



