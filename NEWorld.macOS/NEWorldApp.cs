namespace NEWorld.macOS
{
    internal class NEWorldApp
    {
        private static void Main(string[] args)
        {
            using (var game = new Xenko.Engine.Game())
            {
                game.Run();
            }
        }
    }
}