using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blazorme
{
    public partial class Split : ComponentBase
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        private List<SplitPane> _splitPanes = new List<SplitPane>();

        internal void AddSplitPane(SplitPane splitPane)
        {
            _splitPanes.Add(splitPane);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
        }
    }
}
