using System;
using System.IO;

namespace Zombles
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ZomblesGame game = new ZomblesGame();
            Plugin.SetGame(game);
            game.Run();
            game.Dispose();
        }
    }
}
