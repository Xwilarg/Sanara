using System.Diagnostics;

namespace Sanara;

public static class PathManager
{
    public static string Path => Debugger.IsAttached ? "./" : "/data/";
}
