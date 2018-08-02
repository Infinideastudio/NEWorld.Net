using System;

namespace Core
{
    public static class Path
    {
        public static string Asset(string group) => AppContext.BaseDirectory + "/Assets/" + group + "/";
        
        public static string Modules() => AppContext.BaseDirectory;
    }
}