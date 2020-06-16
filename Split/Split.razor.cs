using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Split;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if(firstRender)
            {
                await JsInterop.InvokeAsync(_jsRuntime,
                    _splitPanes.Select(splitPane => splitPane.ElementReference).ToArray(),
                    new Options
                    {
                        Sizes = _splitPanes.Select(splitPane => splitPane.SizeInPercentage).ToArray(),
                        MinSizes = _splitPanes.Select(splitPane => splitPane.MinSize ?? DefaultMinSize).ToArray(),
                        ExpandToMin = ExpandToMin,
                        GutterSize = GutterSize,
                        GutterAlign = GutterAlign switch
                        {
                            SplitGutterAlign.Start => "start",
                            SplitGutterAlign.Center => "center",
                            SplitGutterAlign.End => "end",
                            _ => "center"
                        },
                        SnapOffset = SnapOffset,
                        DragInterval = DragInterval,
                        Direction = Direction switch
                        {
                            SplitDirection.Horizontal => "horizontal",
                            SplitDirection.Vertical => "vertical",
                            _ => "horizontal"
                        },
                        Cursor = Cursor
                    });
            }
        }
    }
}
