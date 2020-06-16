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
        public int SizeInPercentage { get; set; }

        [Parameter]
        public int? MinSize { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        private ElementReference _elementReference = new ElementReference(Guid.NewGuid().ToString());

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if(_split == null)
            {
                throw new Exception("SplitPane should be a child of Split");
            }
            _split.AddSplitPane(this);
        }
    }
}
