using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Evaluation
{
    class Program
    {
        static void Main(string[] args)
        {
            var seeds = new int[] {
                0xb6ba069,
                0x85f1d06,
                0x7e2b2d9,
                0x3314573,
                0x545c87d,
                0x2ae88ea,
                0x3690649,
                0xb501192,
                0xb46c596,
                0xa3c2e9e
            };

            var types = new[] {
                "sub",
                "bdi"
            };

            int size = 64;
            int humans = 96;
            int zombies = 32;

            var evalDir = "..\\..\\Results\\";

            foreach (var seed in seeds) {
                var ident = String.Format("{0}_{1}_{2}_{3}", seed, size, humans, zombies);

                Console.WriteLine(ident);

                foreach (var type in types) {
                    Console.WriteLine(type);

                    var info = new ProcessStartInfo("Zombles.exe", String.Format("-t {0} -s {1} -w {2} -h {3} -z {4} -d {5} -o {6}",
                        type, seed, size, humans, zombies, 600, evalDir + type + "_" + ident + ".log"));

                    var proc = Process.Start(info);

                    while (!proc.HasExited) {
                        Thread.Sleep(100);
                    }

                    proc.Dispose();
                }

                {
                    var info = new ProcessStartInfo("GraphTool", String.Format("-w {0} -h {1} -y {2} -x {3} -o {4} {5} {6} {7} {8}",
                        1280, 640, "\"Population,0-max,32,8\"", "\"Simulation Time (s),0-max,60,15\"", evalDir + "pop_" + ident + ".png",
                        "\"" + evalDir + "sub_" + ident + ".log,a,b,Survivors [SUB],ffff9900,2\"",
                        "\"" + evalDir + "sub_" + ident + ".log,a,c,Zombies [SUB],ff99ff00,2\"",
                        "\"" + evalDir + "bdi_" + ident + ".log,a,b,Survivors [BDI],ffff0099,2\"",
                        "\"" + evalDir + "bdi_" + ident + ".log,a,c,Zombies [BDI],ff00ff99,2\""));

                    var proc = Process.Start(info);
                    
                    while (!proc.HasExited) {
                        Thread.Sleep(10);
                    }

                    proc.Dispose();

                    info = new ProcessStartInfo("GraphTool", String.Format("-w {0} -h {1} -y {2} -x {3} -o {4} {5} {6}",
                        1280, 640, "\"Frame Time per Agent ([mu]s),0-max,25,5\"", "\"Simulation Time (s),0-max,60,15\"", evalDir + "perf_" + ident + ".png",
                        "\"" + evalDir + "sub_" + ident + ".log,a,d*1000/b,Subsumption,ff9900ff,2\"",
                        "\"" + evalDir + "bdi_" + ident + ".log,a,d*1000/b,BDI,ff0099ff,2\""));

                    proc = Process.Start(info);

                    while (!proc.HasExited) {
                        Thread.Sleep(10);
                    }

                    proc.Dispose();
                }
            }
        }
    }
}
