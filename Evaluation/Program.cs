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
        struct ParsedLog
        {
            public double Time;
            public int Humans;
            public int Zombies;
            public double SimTime;
            public double NavTime;

            public double FrameTime { get { return SimTime + NavTime; } }

            public ParsedLog(String line)
            {
                var split = line.Split(' ');
                
                Time = double.Parse(split[0]);
                Humans = int.Parse(split[1]);
                Zombies = int.Parse(split[2]);
                SimTime = double.Parse(split[3]);
                NavTime = double.Parse(split[4]);
            }
        }

        struct Config
        {
            public int Size;
            public int Humans;
            public int Zombies;

            public int Duration;
        }

        static ParsedLog[] ParseLog(String[] lines)
        {
            return lines
                .Where(x => x.Trim().Length > 0 && !x.StartsWith("#"))
                .Select(x => new ParsedLog(x))
                .ToArray();
        }

        static void Main(string[] args)
        {
            var rand = new Random(0x3314573);

            for (int i = 0; i < 12; ++i) rand.Next();

            var seeds = Enumerable.Range(0, 20)
                .Select(x => rand.Next())
                .ToArray();

            var types = new[] {
                "sub",
                "bdi",
                "bdi2"
            };

            int dur = 600;

            var configs = new[] {
                new Config { Size = 64, Humans = 48, Zombies = 16, Duration = dur },
                new Config { Size = 64, Humans = 96, Zombies = 32, Duration = dur },
                new Config { Size = 64, Humans = 120, Zombies = 8, Duration = dur },
                new Config { Size = 128, Humans = 96, Zombies = 32, Duration = dur },
                new Config { Size = 128, Humans = 120, Zombies = 8, Duration = dur },
                new Config { Size = 128, Humans = 128, Zombies = 128, Duration = dur },
                new Config { Size = 128, Humans = 192, Zombies = 64, Duration = dur },
                new Config { Size = 128, Humans = 224, Zombies = 32, Duration = dur }
            };

            var latexOut = File.CreateText("table.tex");

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

                Dictionary<String, double> avgHumans = new Dictionary<string,double>();
                Dictionary<String, double> avgZombies = new Dictionary<string,double>();
                Dictionary<String, double> avgFrameTime = new Dictionary<string,double>();

                foreach (var type in types) {
                    avgHumans.Add(type, 0.0);
                    avgZombies.Add(type, 0.0);
                    avgFrameTime.Add(type, 0.0);
                }
                
                foreach (var seed in seeds) {
                    var ident = seed.ToString();

                    Console.WriteLine("{0}: {1}", Array.IndexOf(seeds, seed), ident);

                    foreach (var type in types) {
                        Console.WriteLine(type);

                        var logName = evalDir + ident + "_" + type + ".log";
                        var oldLogName = evalDir + type + "_" + ident + ".log";

                        if (File.Exists(oldLogName) && !File.Exists(logName)) {
                            File.Move(oldLogName, logName);
                        }

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

                        bool skip = false;
                        ParsedLog[] data = null;

                        if (File.Exists(logName)) {
                            data = ParseLog(File.ReadAllLines(logName));

                            skip = data.LastOrDefault().Time == duration;
                        }

                        if (!skip) {
                            var info = new ProcessStartInfo("Zombles.exe", String.Format("-t {0} -s {1} -w {2} -h {3} -z {4} -d {5} -o {6}",
                                type, seed, size, humans, zombies, duration, logName));

                            var proc = Process.Start(info);

                            while (!proc.HasExited) {
                                Thread.Sleep(100);
                            }

                            proc.Dispose();

                            data = ParseLog(File.ReadAllLines(logName));
                        }

                        double addHumans = 0.0;
                        double addZombies = 0.0;
                        double addTime = 0.0;

                        double prev = 0.0;
                        foreach (var log in data) {
                            double dt = log.Time - prev;

                            addHumans += log.Humans * dt;
                            addZombies += log.Zombies * dt;
                            addTime += log.FrameTime * dt;

                            prev = log.Time;
                        }

                        avgHumans[type] += addHumans;
                        avgZombies[type] += addZombies;
                        avgFrameTime[type] += addTime;
                    }
                }

                double div = seeds.Length * duration;

                latexOut.WriteLine("{0} & {1:F1} & {2:F1} & {3:F1} & {4:F3} & {5:F3} & {6:F3} \\\\ \\hline",
                    (char) ('A' + Array.IndexOf(configs, config)),
                    avgHumans["bdi"] / div,
                    avgHumans["bdi2"] / div,
                    avgHumans["sub"] / div,
                    avgFrameTime["bdi"] / div,
                    avgFrameTime["bdi2"] / div,
                    avgFrameTime["sub"] / div);

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
                        "\"" + String.Join(";", bdis) + ",a,(d+e)*1000/b,Slow BDI,ff00ccff,2\"",
                        "\"" + String.Join(";", bdi2s) + ",a,(d+e)*1000/b,Fast BDI,ff6666ff,2\"",
                        "\"" + String.Join(";", subs) + ",a,(d+e)*1000/b,Subsump,ffcc00ff,2\""));

                    proc = Process.Start(info);

                    while (!proc.HasExited) {
                        Thread.Sleep(10);
                    }

                    proc.Dispose();
                }
            }

            latexOut.Close();
        }
    }
}
