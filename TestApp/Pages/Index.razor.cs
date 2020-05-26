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
        private IDiff Diff { get; set; }

        public string Body1 { get; set; } = string.Empty;
        public string Preview1 => Markdown.ToHtml(Body1);
        public string Body2 { get; set; } = string.Empty;
        public string Preview2 => Markdown.ToHtml(Body2);

        protected override async Task OnInitializedAsync()
        {
            var first = "Hello World";
            var second = "Hell World!";
            var diff = Diff.GetAsync(first, second);
            var diffHtml = Diff.GetHtmlAsync(first, second, DiffOutputFormat.Row);


            await base.OnInitializedAsync();
        }
    }
}
