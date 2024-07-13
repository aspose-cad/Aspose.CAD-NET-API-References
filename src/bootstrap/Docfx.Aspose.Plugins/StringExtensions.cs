using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docfx.Aspose.Plugins;

public static class StringExtensions
{
    public static string AsSafeGenericName(this string symbolName)
    {
        return symbolName.Replace('`', '-');
    }

    public static string AsUnsafeGenericName(this string symbolName)
    {
        return symbolName.Replace('-', '`');
    }
}
