// 
// NEWorld: Program.cs
// NEWorld: A Free Game with Similar Rules to Minecraft.
// Copyright (C) 2015-2018 NEWorld Team
// 
// NEWorld is free software: you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// NEWorld is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General 
// Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with NEWorld.  If not, see <http://www.gnu.org/licenses/>.
// 

namespace NEWorld
{
    internal class NEWorld
    {
        public NEWorld()
        {
            Window.GetInstance("NEWorld", 852, 480);
        }

        public void Run()
        {
            var fps = 60;
            var shouldLimitFps = true;
            var delayPerFrame = (uint)(1000 / fps - 0.5);
            var window = Window.GetInstance("NEWorld", 852, 480);
            var game = new GameScene("TestWorld", window);
            while (!window.ShouldQuit())
            {
                // Update
                window.PollEvents();
                // Render
                game.Render();
                window.SwapBuffers();
                if (shouldLimitFps) 
                    SDL2.SDL.SDL_Delay(delayPerFrame);
            }
        }

        public static void Main(string[] args)
        {
            var instance = new NEWorld();
            instance.Run();
        }
    }
}