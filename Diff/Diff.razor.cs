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
        [Inject]
        private IJSRuntime _jsRuntime { get; set; }

        [Parameter]
        public string FirstString { get; set; }

        [Parameter]
        public string SecondString { get; set; }

        public enum EDiffType
        {
            Html,
            SideBySide,
            LineByLine
        }
        [Parameter]
        public EDiffType DiffType { get; set; }

        private string _diff { get; set; } = string.Empty;


        protected override async Task OnInitializedAsync()
        {
            _diff = await DiffJsInterop.Invoke(_jsRuntime, FirstString, SecondString);

            await base.OnInitializedAsync();
        }

    }
}



