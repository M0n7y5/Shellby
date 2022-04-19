using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shellby.ShellFormatters
{
    public class JustBytesFormatter : IShellFormatter
    {
        public string Name => "Just Shell";

        public string Format(List<(List<byte> binary, string comment)> data)
        {
            var str = new StringBuilder(400);

            foreach (var item in data)
            {
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

            return str.ToString();
        }
    }
}