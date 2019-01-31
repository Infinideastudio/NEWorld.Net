using System;
using Xenko.Core.Diagnostics;

namespace Core
{
    public static class LogPort
    {
        public static Logger Logger { private get; set; }

        public static void Debug(string str)
        {
            if (Logger != null)
                Logger.Debug(str);
            else
                Console.WriteLine(str);
        }
    }
}