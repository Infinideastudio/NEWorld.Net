namespace NEWorld.Linux
{
    class NEWorldApp
    {
        static void Main(string[] args)
        {
            using (var game = new Xenko.Engine.Game())
            {
                game.Run();
            }
        }
    }
}
