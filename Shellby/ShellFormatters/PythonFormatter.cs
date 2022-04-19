using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shellby.ShellFormatters
{
    public class PythonFormatter : IShellFormatter
    {
        public string Name => "Python";

        public string Format(List<(List<byte> binary, string comment)> data)
        {
            var str = new StringBuilder(400);

            str.AppendLine("shellcode = \\");

            foreach (var item in data)
            {
                str.Append("b\"");

                foreach (var b in item.binary)
                {
                    var cbyte = b.ToString("x");

                    str.Append($"\\x{cbyte}");
                }
                str.Append("\" \\");

                if (item.comment.StartsWith("//"))
                    str.AppendLine($" # {item.comment}");
                else
                    str.AppendLine($" # {item.comment}");
            }

            return str.ToString();
        }
    }
}