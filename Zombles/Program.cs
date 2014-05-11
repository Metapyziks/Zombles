using System;
using System.IO;

namespace Zombles
{
    public class Program
    {
        public static int Seed { get; set; }

        public static int WorldSize { get; set; }

        public static int SurvivorCount { get; set; }

        public static int ZombieCount { get; set; }

        public static double Duration { get; set; }

        public static String LogName { get; set; }

        public static bool Subsumption { get; set; }

        public static bool Deliberative { get; set; }

        public static bool FastDeliberative { get; set; }

        public static bool PlayerControl { get; set; }

        [STAThread]
        public static void Main(string[] args)
        {
            Seed = (int) (DateTime.Now.Ticks % int.MaxValue) + 1;

            WorldSize = 128;
            SurvivorCount = 96;
            ZombieCount = 32;
            Duration = double.PositiveInfinity;
            Subsumption = false;
            Deliberative = false;
            FastDeliberative = false;
            PlayerControl = false;

            var logFileFormat = String.Format("{{0}}_{0}_{1}_{2}_{3}.log",
                Program.WorldSize, Program.SurvivorCount, Program.ZombieCount,
                DateTime.Now.ToString("d_MMM_yy_HH_mm_ss"));

            LogName = String.Format(logFileFormat, Program.Subsumption ? "sub" : "bdi");
        
            MainWindow game = new MainWindow();
            Plugin.SetGame(game);
            game.Run();
            game.Dispose();
        }
    }
}
