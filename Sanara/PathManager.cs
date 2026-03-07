using System.Diagnostics;

namespace Sanara;

public static class PathManager
{
    public static string Path => (Debugger.IsAttached || Environment.GetEnvironmentVariable("TEST") == "1") ? "./" : "/data/";
}
