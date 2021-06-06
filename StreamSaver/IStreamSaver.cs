using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazorme
{
    public interface IStreamSaver
    {
        Task CopyToAsync(Stream stream);
    }
}
