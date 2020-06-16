using System;
using System.Collections.Generic;
using System.Text;

namespace Split
{
    internal class Options
    {
        internal int[] Sizes { get; set; }
        internal int[] MinSizes { get; set; }
        internal bool ExpandToMin { get; set; }
        internal int GutterSize { get; set; }
        internal string GutterAlign { get; set; }
        internal int SnapOffset { get; set; }
        internal int DragInterval { get; set; }
        internal string Direction { get; set; }
        internal string Cursor { get; set; }
    }
}
