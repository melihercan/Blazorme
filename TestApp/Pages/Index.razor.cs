using Blazorme;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp.Pages
{
    public partial class Index : ComponentBase
    {
        [Inject]
        private IDiffApi _diff { get; set; }

        public string Body1 { get; set; } = string.Empty;
        public string Preview1 => Markdown.ToHtml(Body1);
        public string Body2 { get; set; } = string.Empty;
        public string Preview2 => Markdown.ToHtml(Body1);




        protected override async Task OnInitializedAsync()
        {
            var x = await _diff.Get("Hello World", "Hell World!");

            await base.OnInitializedAsync();
        }


        private void RefreshSelected(MouseEventArgs e)
        {

        }
    }
}
