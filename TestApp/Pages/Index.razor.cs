using Blazorme;
using Microsoft.AspNetCore.Components;
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
        protected override async Task OnInitializedAsync()
        {
            var x = await _diff.Get("Hello World", "Hell World!");

            await base.OnInitializedAsync();
        }

    }
}
