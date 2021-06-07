using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp.Models
{
    public class StreamSaverModel
    {
        public string Filename { get; set; }
        public bool FilenameDisabled { get; set; }
        public string Text { get; set; }
        public int NumLoremIpsum { get; set; }
    }
}
