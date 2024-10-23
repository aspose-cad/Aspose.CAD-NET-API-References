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
    
    public static string ReplaceLast(this string symbolName, string old, string @new)
    {
        var index = symbolName.LastIndexOf(old);
        if (index >= 0)
        {
            return symbolName.Remove(index) + @new + symbolName.Substring(index + old.Length);
        }
        
        return symbolName;
    }
}
