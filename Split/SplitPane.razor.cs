﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blazorme
{
    public partial class SplitPane
    {
        [CascadingParameter]
        private Split _split { get; set; }

        [Parameter]
        public int SizeInPercentage { get; set; }

        [Parameter]
        public int? MinSize { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        public ElementReference ElementReference;

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
