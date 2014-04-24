using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Evaluation
{
    class Program
    {
        struct Config
        {
            public int Size;
            public int Humans;
            public int Zombies;

            public int Duration;
        }

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
                "bdi",
                "bdi2"
            };

            int dur = 600;

            var configs = new[] {
                new Config { Size = 64, Humans = 96, Zombies = 32, Duration = dur },
                new Config { Size = 128, Humans = 96, Zombies = 32, Duration = dur },
                new Config { Size = 128, Humans = 128, Zombies = 128, Duration = dur },
                new Config { Size = 128, Humans = 192, Zombies = 64, Duration = dur }
            };

            foreach (var config in configs) {
                int size = config.Size;
                int humans = config.Humans;
                int zombies = config.Zombies;
                int duration = config.Duration;

                var subs = new List<String>();
                var bdis = new List<String>();
                var bdi2s = new List<String>();

                var evalDir = String.Format("..\\..\\Results\\{0}_{1}_{2}\\", size, humans, zombies);

                if (!Directory.Exists(evalDir)) {
                    Directory.CreateDirectory(evalDir);
                }

                foreach (var seed in seeds) {
                    var ident = String.Format("{0}_{1}_{2}_{3}", seed, size, humans, zombies);

                    Console.WriteLine("{0}: {1}", Array.IndexOf(seeds, seed), ident);

                    foreach (var type in types) {
                        Console.WriteLine(type);

                        var logName = evalDir + type + "_" + ident + ".log";

                        switch (type) {
                            case "sub":
                                subs.Add(logName);
                                break;
                            case "bdi":
                                bdis.Add(logName);
                                break;
                            case "bdi2":
                                bdi2s.Add(logName);
                                break;
                        }

                        if (File.Exists(logName)) {
                            var data = File.ReadAllLines(logName);
                            if (data.Where(x => x.Trim().Length > 0).Any(x => x.StartsWith(duration + " "))) {
                                continue;
                            }
                        }

                        var info = new ProcessStartInfo("Zombles.exe", String.Format("-t {0} -s {1} -w {2} -h {3} -z {4} -d {5} -o {6}",
                            type, seed, size, humans, zombies, duration, logName));

                        var proc = Process.Start(info);

                        while (!proc.HasExited) {
                            Thread.Sleep(100);
                        }

                        proc.Dispose();
                    }
                }

                {
                    var info = new ProcessStartInfo("GraphTool", String.Format("-w {0} -h {1} -y {2} -x {3} -o {4} {5} {6} {7} {8} {9} {10}",
                        1280, 640, "\"Population,0-max," + (humans / 8) + "," + (humans / 32) + "\"", "\"Simulation Time (s),0-max,60,15\"", evalDir + "population.png",
                        "\"" + String.Join(";", bdis) + ",a,b,Survivors [Slow BDI],ffff0099,2\"",
                        "\"" + String.Join(";", bdi2s) + ",a,b,Survivors [Fast BDI],ffff6666,2\"",
                        "\"" + String.Join(";", subs) + ",a,b,Survivors [Subsump],ffff9900,2\"",
                        "\"" + String.Join(";", bdis) + ",a,c,Zombies [Slow BDI],ff00ff99,2\"",
                        "\"" + String.Join(";", bdi2s) + ",a,c,Zombies [Fast BDI],ff66ff66,2\"",
                        "\"" + String.Join(";", subs) + ",a,c,Zombies [Subsump],ff99ff00,2\""));

                    var proc = Process.Start(info);

                    while (!proc.HasExited) {
                        Thread.Sleep(10);
                    }

                    proc.Dispose();

                    info = new ProcessStartInfo("GraphTool", String.Format("-w {0} -h {1} -y {2} -x {3} -o {4} {5} {6} {7}",
                        1280, 640, "\"Frame Time per Agent ([mu]s),0-max,10,2\"", "\"Simulation Time (s),0-max,60,15\"", evalDir + "performance.png",
                        "\"" + String.Join(";", bdis) + ",a,(d)*1000/b,Slow BDI,ff00ccff,2\"",
                        "\"" + String.Join(";", bdi2s) + ",a,(d)*1000/b,Fast BDI,ff6666ff,2\"",
                        "\"" + String.Join(";", subs) + ",a,(d)*1000/b,Subsump,ffcc00ff,2\""));

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
