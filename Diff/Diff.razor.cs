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
            Inline,
            LineByLine,
            SideBySide,
        }

        [Parameter]
        public EOutputFormat OutputFormat { get; set; } = EOutputFormat.Inline;

        [Parameter]
        public string Title { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            _diff = await DiffJsInterop.Invoke(_jsRuntime, FirstString, SecondString, OutputFormat, Title );

            await base.OnInitializedAsync();
        }

    }
}



