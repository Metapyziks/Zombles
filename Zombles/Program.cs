using System;
using System.IO;

namespace Zombles
{
    public class Program
    {
        // 0: 0xb6ba069
        // 1: 0x85f1d06

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
            Deliberative = !Subsumption;
            FastDeliberative = false;
            PlayerControl = true;

            var logFileFormat = String.Format("{{0}}_{0}_{1}_{2}_{3}.log",
                Program.WorldSize, Program.SurvivorCount, Program.ZombieCount,
                DateTime.Now.ToString("d_MMM_yy_HH_mm_ss"));

            LogName = String.Format(logFileFormat, Program.Subsumption ? "sub" : "bdi");
        
            for (int i = 0; i < args.Length; ++i) {
                switch (args[i]) {
                    case "-s":
                    case "--seed":
                        Seed = int.Parse(args[++i]);
                        break;
                    case "-w":
                    case "--size":
                        WorldSize = int.Parse(args[++i]);
                        break;
                    case "-h":
                    case "--humans":
                        SurvivorCount = int.Parse(args[++i]);
                        break;
                    case "-z":
                    case "--zombies":
                        ZombieCount = int.Parse(args[++i]);
                        break;
                    case "-d":
                    case "--duration":
                        Duration = double.Parse(args[++i]);
                        break;
                    case "-o":
                    case "--output":
                        LogName = args[++i];
                        break;
                    case "-t":
                    case "--type":
                        Subsumption = args[++i] == "sub";
                        FastDeliberative = args[i] == "bdi2";
                        break;
                }
            }

            Deliberative = !Subsumption;

            MainWindow game = new MainWindow();
            Plugin.SetGame(game);
            game.Run();
            game.Dispose();
        }
    }
}
