#define SUBSUMPTIVE

using System;
using System.Linq;
using System.Diagnostics;

using OpenTK;

using Zombles.Graphics;
using Zombles.Geometry;
using Zombles.Entities;

using Zombles.Scripts.Entities;
using System.IO;

namespace Zombles.Scripts
{
    public class ZomblesPlugin : Plugin
    {
        private double _lastAliveCheck;
        private int _lastSurvivors;
        private int _lastZombies;

#if SUBSUMPTIVE
        private String _logFileName = "subsumptive.log";
#else
        private String _logFileName = "original.log";
#endif

        protected override void OnInitialize()
        {
            Entity.Register("human", ent => {
                ent.AddComponent<RenderAnim>();
                ent.AddComponent<Collision>()
                    .SetDimentions(0.5f, 0.5f)
                    .Model = CollisionModel.Repel | CollisionModel.Entity;
                ent.AddComponent<Movement>();
                ent.AddComponent<Health>();
            });

            Entity.Register("survivor", "human", ent => {
                ent.AddComponent<Survivor>();
                ent.AddComponent<RouteNavigation>();
#if SUBSUMPTIVE
                ent.AddComponent<SubsumptionStack>()
                    .Push<Entities.Behaviours.Wander>()
                    .Push<Entities.Behaviours.FollowRoute>()
                    .Push<Entities.Behaviours.Flee>()
                    .Push<Entities.Behaviours.Mob>()
                    .Push<Entities.Behaviours.SelfDefence>();
#else
                ent.AddComponent<SurvivorAI>();
#endif
            });

            Entity.Register("zombie", "human", ent => {
                ent.AddComponent<Zombie>();
                ent.AddComponent<ZombieAI>();
            });

            Entity.Register("crate", ent => {
                ent.AddComponent<StaticTile>();
                ent.AddComponent<Render3D>()
                    .Model = EntityModel.Get("models", "deco", "crate",
                        Tools.Random.NextDouble() < 0.5 ? "large" : "small");
            });

            MainWindow.SetScene(new GameScene(Game));

            File.Create(_logFileName).Close();
        }

        protected override void OnCityGenerated()
        {
            GameScene scene = MainWindow.CurrentScene as GameScene;
            World world = scene.World;
            Random rand = Tools.Random;

            int count = 512;
            int zoms = Math.Max(count / 4, 8);

            Func<Vector2> randPos = () => {
                Vector2 pos;
                do {
                    pos = new Vector2(rand.NextSingle() * world.Width, rand.NextSingle() * world.Height);
                } while (world.GetTile(pos).FloorHeight > 0);
                return pos;
            };

            for (int i = 0; i < count - zoms; ++i) {
                Entity surv = Entity.Create(world, "survivor");
                surv.Position2D = randPos();
                surv.Spawn();
            }

            for (int i = 0; i < zoms; ++i) {
                Entity zomb = Entity.Create(world, "zombie");
                zomb.Position2D = randPos();
                zomb.Spawn();
            }
        }

        //protected override void OnThink(double dt)
        //{
        //    base.OnThink(dt);

        //    if (MainWindow.Time - _lastAliveCheck >= 1.0 && Scene is GameScene) {
        //        World world = (Scene as GameScene).World;

        //        _lastAliveCheck = MainWindow.Time;

        //        int survivors = world.Entities.Where(x => x.HasComponent<Survivor>())
        //        .Count(x => x.GetComponent<Health>().IsAlive);

        //        int zombies = world.Entities.Where(x => x.HasComponent<Zombie>())
        //            .Count(x => x.GetComponent<Health>().IsAlive);

        //        if (survivors != _lastSurvivors || zombies != _lastZombies) {
        //            _lastSurvivors = survivors;
        //            _lastZombies = zombies;
        //            var log = String.Format("{0} {1} {2}", _lastAliveCheck, survivors, zombies);
        //            Debug.WriteLine(log);
        //            File.AppendAllText(_logFileName, log + Environment.NewLine);
        //        }
        //    }
        //}
    }
}
