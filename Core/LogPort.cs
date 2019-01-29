namespace Core
{
    public static class LogPort
    {
        public static Xenko.Core.Diagnostics.Logger Logger { get; set; }

        public static void Debug(string str)
        {
            if (Logger != null)
            {
                Logger.Debug(str);
            }
            else
            {
                System.Console.WriteLine(str);
            }
        }
    }
}
