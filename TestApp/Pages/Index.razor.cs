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
        private IDiff DiffApi { get; set; }

        public string Body1 { get; set; } = string.Empty;
        public string Preview1 => Markdown.ToHtml(Body1);
        public string Body2 { get; set; } = string.Empty;
        public string Preview2 => Markdown.ToHtml(Body2);

        protected override async Task OnInitializedAsync()
        {
            var firstInput = "Hello World";
            var secondInput = "Hell World!";
            var diff = DiffApi.GetAsync(firstInput, secondInput);
            var diffHtml = DiffApi.GetHtmlAsync(firstInput, secondInput, 
                DiffInputTitle.First, DiffInputTitle.Second, 
                DiffOutputFormat.Row);


            await base.OnInitializedAsync();
        }
    }
}
