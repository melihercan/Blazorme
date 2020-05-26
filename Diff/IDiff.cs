using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Blazorme
{

    public interface IDiff
    {

        public Task<string> GetAsync(string first, string second);
        public Task<string> GetHtmlAsync(string first, string second, DiffOutputFormat outputFormat);
    }
}
