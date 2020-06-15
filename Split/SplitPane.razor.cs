using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blazorme
{
    public partial class SplitPane : ComponentBase
    {
        [CascadingParameter]
        private Split _split { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }


        protected override void OnInitialized()
        {
            base.OnInitialized();

            if(_split == null)
            {
                throw new Exception("SplitPane should be child of Split");
            }
            _split.AddSplitPane(this);
        }
    }
}
