using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Blazorme
{
    public interface IDiffApi
    {
        public ValueTask<string> Get(string first, string second);
        public string GetHtml(string first, string second);
    }
}
