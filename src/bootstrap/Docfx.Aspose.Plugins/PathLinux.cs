using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docfx.Aspose.Plugins;

public static class PathLinux
{
    public static string Combine(params string[] paths)
    {
        return Path.Combine(paths).Replace("\\", "/");
    }
}
