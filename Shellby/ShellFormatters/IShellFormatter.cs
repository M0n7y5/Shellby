using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shellby.ShellFormatters
{
    public interface IShellFormatter
    {
        public string Name { get; }

        public string Format(List<(List<byte> binary, string comment)> data);
    }
}