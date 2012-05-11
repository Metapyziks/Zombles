using System;
using System.IO;

using ResourceLib;

namespace Zombles
{
    public class Program
    {
        [STAThread]
        public static void Main( string[] args )
        {
            ZomblesGame game = new ZomblesGame();
            game.Run();
            game.Dispose();
        }
    }
}
