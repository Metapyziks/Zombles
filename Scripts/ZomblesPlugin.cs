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
                ent.AddComponent<SubsumptionStack>()
                    .Push<Entities.Behaviours.Wander>()
                    .Push<Entities.Behaviours.FollowRoute>()
                    .Push<Entities.Behaviours.Flee>()
                    .Push<Entities.Behaviours.Mob>()
                    .Push<Entities.Behaviours.SelfDefence>();
            });

            Entity.Register("zombie", "human", ent => {
                ent.AddComponent<Zombie>();
                ent.AddComponent<ZombieAI>();
            });

            Entity.Register("crate", ent => {
                ent.AddComponent<StaticTile>();
                var render3d = ent.AddComponent<Render3D>();
                
                render3d.Model = EntityModel.Get("models", "deco", "crate",
                    Tools.Random.NextDouble() < 0.5 ? "large" : "small");

                render3d.Skin = Tools.Random.Next(render3d.Model.Skins);
            });

            MainWindow.SetScene(new GameScene(Game));
        }

        protected override void OnCityGenerated()
        {
            GameScene scene = MainWindow.CurrentScene as GameScene;
            World world = scene.World;
            Random rand = new Random(0x4812f34e);

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
    }
}
