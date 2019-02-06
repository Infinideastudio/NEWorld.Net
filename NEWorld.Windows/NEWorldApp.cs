namespace NEWorld.Windows
{
    internal class NEWorldApp
    {
        private static void Main(string[] args)
        {
            // TODO: Remove Later when Launching Server with Client is Possible
            var server = System.Diagnostics.Process.Start("NEWorldShell.exe");
            using (var game = new Xenko.Engine.Game())
            {
                game.Run();
            }
            server?.Kill();
        }
    }
}