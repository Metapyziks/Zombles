using System;
using System.IO;

namespace Zombles
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            MainWindow game = new MainWindow();
            Plugin.SetGame(game);
            game.Run();
            game.Dispose();
        }
    }
}
