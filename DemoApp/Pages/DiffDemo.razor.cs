using Blazorme;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp.Pages
{
    public partial class DiffDemo : ComponentBase
    {
        [Inject]
        private IDiff _diffApi { get; set; }

        public string Body1 { get; set; } = string.Empty;
        public string Preview1 => Markdown.ToHtml(Body1);
        public string Body2 { get; set; } = string.Empty;
        public string Preview2 => Markdown.ToHtml(Body2);

        protected override async Task OnInitializedAsync()
        {
            // API usage example.
           var firstInput = "Hello World";
            var secondInput = "Hell World!";
            var diff = await _diffApi.GetAsync(firstInput, secondInput);
            var diffHtml = await _diffApi.GetHtmlAsync(firstInput, secondInput,
                DiffInputTitle.First, DiffInputTitle.Second,
                DiffOutputFormat.Row);
            Console.WriteLine("diff:");
            Console.WriteLine(diff);
            Console.WriteLine("diffHtml:");
            Console.WriteLine(diffHtml);

            await base.OnInitializedAsync();
        }
    }
}
