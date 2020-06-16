using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Blazorme
{
    public partial class Split : ComponentBase
    {
        [Inject]
        private IJSRuntime _jsRuntime { get; set; }

        [Parameter]
        public int DefaultMinSize { get; set; } = 100;

        [Parameter]
        public bool ExpandToMin { get; set; } = false;

        [Parameter]
        public int GutterSize { get; set; } = 10;

        [Parameter]
        public SplitGutterAlign GutterAlign { get; set; } = SplitGutterAlign.Center;

        [Parameter]
        public int SnapOffset { get; set; } = 30;

        [Parameter]
        public int DragInterval { get; set; } = 1;

        [Parameter]
        public SplitDirection Direction { get; set; } = SplitDirection.Horizontal;

        [Parameter]
        public string Cursor { get; set; } = "col-resize";


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

            //// TODO: INVOKE JS INTEROP HERE
        }
    }
}
