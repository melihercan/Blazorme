using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using BlazormeSplit;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazorme
{
    public partial class Split
    {
        [Inject]
        private IJSRuntime _jsRuntime { get; set; } = default!;

        [Parameter]
        public int DefaultMinSize { get; set; } = 100;

        [Parameter]
        public bool ExpandToMin { get; set; } = false;

        [Parameter]
        public int GutterSize { get; set; } = 10;

        [Parameter]
        public SplitGutterAlign GutterAlign { get; set; } = SplitGutterAlign.Center;

        [Parameter]
        public string GutterColor { get; set; } = "#cfcfcf";


        [Parameter]
        public int SnapOffset { get; set; } = 30;

        [Parameter]
        public int DragInterval { get; set; } = 1;

        [Parameter]
        public SplitDirection Direction { get; set; } = SplitDirection.Horizontal;

        [Parameter]
        public string Cursor { get; set; } = string.Empty;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        private List<SplitPane> _splitPanes = new List<SplitPane>();


        internal void AddSplitPane(SplitPane splitPane)
        {
            _splitPanes.Add(splitPane);
        }

        // Resolved on every use rather than assigned to the Cursor parameter in OnInitialized.
        // Blazor owns parameter properties, and OnInitialized runs once, so the old code left the
        // cursor describing whatever Direction happened to be at mount time.
        private string EffectiveCursor =>
            string.IsNullOrEmpty(Cursor)
                ? (Direction == SplitDirection.Horizontal ? "col-resize" : "row-resize")
                : Cursor;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if(firstRender)
            {
                await JsInterop.InvokeAsync(_jsRuntime,
                    _splitPanes.Select(splitPane => splitPane.ElementReference).ToArray(),
                    new Options
                    {
                        Sizes = _splitPanes.All(splitPane => splitPane.SizeInPercentage == 0) ? null : 
                            _splitPanes.Select(splitPane => splitPane.SizeInPercentage).ToArray(),
                        MinSize = _splitPanes.Select(splitPane => splitPane.MinSize ?? DefaultMinSize).ToArray(),
                        ExpandToMin = ExpandToMin,
                        GutterSize = GutterSize,
                        GutterAlign = GutterAlign.ToString().ToLowerInvariant(),
                        SnapOffset = SnapOffset,
                        DragInterval = DragInterval,
                        Direction = Direction.ToString().ToLowerInvariant(),
                        Cursor = EffectiveCursor
                    });
            }
        }
    }
}
