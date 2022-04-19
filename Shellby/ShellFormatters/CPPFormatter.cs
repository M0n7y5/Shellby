using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shellby.ShellFormatters
{
    public class CPPFormatter : IShellFormatter
    {
        public string Name => "C++";

        public string Format(List<(List<byte> binary, string comment)> data)
        {
            var str = new StringBuilder(400);

            str.AppendLine("constexpr unsigned char shellcode[] = {");

            foreach (var item in data)
            {
                str.Append("    ");
                foreach (var b in item.binary)
                {
                    var cbyte = b.ToString("x2");

                    str.Append($"0x{cbyte}, ");
                }

                if (item.comment.StartsWith("//"))
                    str.AppendLine($" {item.comment}");
                else
                    str.AppendLine($" // {item.comment}");
            }

            str.AppendLine("};");
            return str.ToString();
        }
    }
}