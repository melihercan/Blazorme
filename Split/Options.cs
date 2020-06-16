using System;
using System.Collections.Generic;
using System.Text;

namespace BlazormeSplit
{
    public class Options
    {
        public int[] Sizes { get; set; }
        public int[] MinSize { get; set; }
        public bool ExpandToMin { get; set; }
        public int GutterSize { get; set; }
        public string GutterAlign { get; set; }
        public int SnapOffset { get; set; }
        public int DragInterval { get; set; }
        public string Direction { get; set; }
        public string Cursor { get; set; }
    }
}
