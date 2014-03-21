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
        public const int WorldSize = 128;
        public const int AgentCount = (WorldSize * WorldSize) / 64;
        public const int ZombieCount = AgentCount / 4 < 8 ? 8 : AgentCount / 4;
        public const int SurvivorCount = AgentCount - ZombieCount;
        public const int Seed = 0xb6ba069;

        public const bool Subsumption = true;
        public const bool Deliberative = !Subsumption;

        private double _lastAliveCheck;
        private int _lastSurvivors;
        private int _lastZombies;
        
        private static readonly String _logFileFormat = String.Format("{{0}}_{0}_{1}_{2}.log", WorldSize, SurvivorCount, ZombieCount);

        private static readonly String _logFileName = String.Format(_logFileFormat, Subsumption ? "sub" : "bdi");

        protected override void OnInitialize()
        {
            var rand = new Random(Seed);

            Entity.Register("selection marker", ent => {
                ent.AddComponent<Render3D>()
                    .SetModel(EntityModel.Get("models", "selection"))
                    .SetOffset(new Vector3(0f, 1f / 8f, 0f))
                    .SetScale(0.5f);
                ent.ThinkExtended += (sender, e) => {
                    ent.GetComponent<Render3D>()
                        .SetRotation((float) (Math.PI * MainWindow.Time));
                };
            });

            Entity.Register("human", ent => {
                ent.AddComponent<RenderAnim>();
                ent.AddComponent<Collision>()
                    .SetDimentions(0.5f, 0.5f)
                    .SetModel(CollisionModel.Repel | CollisionModel.Entity);
                ent.AddComponent<Movement>();
                ent.AddComponent<Health>();
            });

            Entity.Register("survivor", "human", ent => {
                ent.AddComponent<Survivor>();
                if (Subsumption) {
                    ent.AddComponent<SubsumptionStack>()
                        .Push<Entities.Behaviours.Wander>()
                        .Push<Entities.Behaviours.SeekRefuge>()
                        .Push<Entities.Behaviours.BreakCrates>()
                        .Push<Entities.Behaviours.MoveTowardsWood>()
                        .Push<Entities.Behaviours.PickupWood>()
                        .Push<Entities.Behaviours.Flee>()
                        .Push<Entities.Behaviours.VacateDangerousBlocks>()
                        .Push<Entities.Behaviours.Mob>()
                        .Push<Entities.Behaviours.SelfDefence>()
                        .Push<Entities.Behaviours.DropWood>();
                } else {
                    ent.AddComponent<DeliberativeAI>()
                        .AddDesire<Entities.Desires.Wander>()
                        .AddDesire<Entities.Desires.ThreatAvoidance>()
                        .AddDesire<Entities.Desires.WallAvoidance>()
                        .AddDesire<Entities.Desires.Migration>()
                        .AddDesire<Entities.Desires.Mobbing>()
                        .AddDesire<Entities.Desires.Barricading>();
                }
            });

            Entity.Register("zombie", "human", ent => {
                ent.AddComponent<Zombie>();
                ent.AddComponent<ZombieAI>();
            });

            Entity.Register("crate", ent => {
                ent.AddComponent<StaticTile>();
                ent.AddComponent<Health>();
                ent.AddComponent<WoodenBreakable>();
                ent.AddComponent<Collision>()
                    .SetDimentions(1.125f, 1.125f)
                    .SetModel(CollisionModel.Entity);
                ent.AddComponent<Render3D>()
                    .SetRotation(rand.NextSingle(-MathHelper.Pi / 16f, MathHelper.Pi / 16f))
                    .SetScale(
                        rand.NextSingle(0.75f, 0.9f),
                        rand.NextSingle(0.75f, 0.9f),
                        rand.NextSingle(0.75f, 0.9f));
            });

            Entity.Register("small crate", "crate", ent => {
                ent.GetComponent<Health>()
                    .SetMaximum(50)
                    .Revive();
                ent.GetComponent<WoodenBreakable>()
                    .SetMinPlanks(2)
                    .SetMaxPlanks(3);
                ent.GetComponent<Render3D>()
                    .SetModel(EntityModel.Get("models", "deco", "crate", "small"))
                    .SetSkin(rand);
            });

            Entity.Register("large crate", "crate", ent => {
                ent.GetComponent<Health>()
                    .SetMaximum(100)
                    .Revive();
                ent.GetComponent<WoodenBreakable>()
                    .SetMinPlanks(3)
                    .SetMaxPlanks(6);
                ent.GetComponent<Render3D>()
                    .SetModel(EntityModel.Get("models", "deco", "crate", "large"))
                    .SetSkin(rand);
            });

            Entity.Register("plank", ent => {
                ent.AddComponent<Plank>();
                ent.AddComponent<Render3D>()
                    .SetModel(EntityModel.Get("models", "deco", "plank"))
                    .SetSkin(rand)
                    .SetScale(
                        rand.NextSingle(0.75f, 0.9f),
                        rand.NextSingle(0.75f, 0.9f),
                        rand.NextSingle(0.75f, 0.9f));
            });

            Entity.Register("wood pile", ent => {
                ent.AddComponent<Collision>()
                    .SetDimentions(1.125f, 1.125f)
                    .SetModel(CollisionModel.Entity);

                var pile = ent.AddComponent<WoodPile>();

                int count = rand.Next(8) + 1;
                for (int i = 0; i < count; ++i) {
                    pile.AddPlank(Entity.Create(ent.World, "plank"));
                }
            });

            MainWindow.SetScene(new GameScene(Game));
        }

        protected override void OnCityGenerated()
        {
            GameScene scene = MainWindow.CurrentScene as GameScene;
            World world = scene.World;
            Random rand = new Random(Seed);

            Func<Vector2> randPos = () => {
                Vector2 pos;
                do {
                    pos = new Vector2(rand.NextSingle() * world.Width, rand.NextSingle() * world.Height);
                } while (world.GetTile(pos).IsSolid);
                return pos;
            };

            for (int i = 0; i < SurvivorCount; ++i) {
                Entity surv = Entity.Create(world, "survivor");
                surv.Position2D = randPos();

                surv.Spawn();
            }

            for (int i = 0; i < ZombieCount; ++i) {
                Entity zomb = Entity.Create(world, "zombie");
                zomb.Position2D = randPos();
                zomb.Spawn();
            }
        }
        protected override void OnThink(double dt)
        {
            base.OnThink(dt);

            if (MainWindow.Time - _lastAliveCheck >= 1.0 && Scene is GameScene) {
                World world = (Scene as GameScene).World;

                _lastAliveCheck = MainWindow.Time;

                int survivors = world.Entities.Where(x => x.HasComponent<Survivor>())
                .Count(x => x.GetComponent<Health>().IsAlive);

                int zombies = world.Entities.Where(x => x.HasComponent<Zombie>())
                    .Count(x => x.GetComponent<Health>().IsAlive);

                if (survivors != _lastSurvivors || zombies != _lastZombies) {
                    _lastSurvivors = survivors;
                    _lastZombies = zombies;
                    var log = String.Format("{0} {1} {2}", _lastAliveCheck, survivors, zombies);
                    Debug.WriteLine(log);
                    File.AppendAllText(_logFileName, log + Environment.NewLine);
                }
            }
        }
    }
}
