using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Blazorme
{
    public partial class Diff
    {
        private string _diff { get; set; } = string.Empty;

        [Inject]
        private IDiff _diffApi { get; set; }

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

        [Parameter]
        public DiffStyle Style { get; set; } = DiffStyle.Word;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _diff = await _diffApi.GetHtmlAsync(FirstInput, SecondInput, FirstTitle, SecondTitle, OutputFormat, Style);
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            _diff = await _diffApi.GetHtmlAsync(FirstInput, SecondInput, FirstTitle, SecondTitle, OutputFormat, Style);
        }
    }
}



