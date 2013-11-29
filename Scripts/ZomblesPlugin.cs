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
        protected override void OnInitialize()
        {
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
                ent.AddComponent<RouteNavigation>();
                ent.AddComponent<SubsumptionStack>()
                    .Push<Entities.Behaviours.Wander>()
                    .Push<Entities.Behaviours.BreakCrates>()
                    .Push<Entities.Behaviours.MoveTowardsWood>()
                    .Push<Entities.Behaviours.PickupWood>()
                    .Push<Entities.Behaviours.FollowRoute>()
                    .Push<Entities.Behaviours.Flee>()
                    .Push<Entities.Behaviours.VacateDangerousBlocks>()
                    .Push<Entities.Behaviours.Mob>()
                    .Push<Entities.Behaviours.SelfDefence>()
                    .Push<Entities.Behaviours.DropWood>();
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
                    .SetModel(CollisionModel.Box);
                ent.AddComponent<Render3D>()
                    .SetRotation(Tools.Random.NextSingle(-MathHelper.Pi / 16f, MathHelper.Pi / 16f))
                    .SetScale(
                        Tools.Random.NextSingle(0.75f, 0.9f),
                        Tools.Random.NextSingle(0.75f, 0.9f),
                        Tools.Random.NextSingle(0.75f, 0.9f));
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
                    .SetSkin(Tools.Random);
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
                    .SetSkin(Tools.Random);
            });

            Entity.Register("plank", ent => {
                ent.AddComponent<Plank>();
                ent.AddComponent<Render3D>()
                    .SetModel(EntityModel.Get("models", "deco", "plank"))
                    .SetSkin(Tools.Random)
                    .SetScale(
                        Tools.Random.NextSingle(0.75f, 0.9f),
                        Tools.Random.NextSingle(0.75f, 0.9f),
                        Tools.Random.NextSingle(0.75f, 0.9f));
            });

            Entity.Register("wood pile", ent => {
                var pile = ent.AddComponent<WoodPile>();

                int count = Tools.Random.Next(8) + 1;
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
            Random rand = Tools.Random;

            int count = (world.Width * world.Height) / 32;
            int zoms = 0; // Math.Max(count / 4, 8);

            Func<Vector2> randPos = () => {
                Vector2 pos;
                do {
                    pos = new Vector2(rand.NextSingle() * world.Width, rand.NextSingle() * world.Height);
                } while (world.GetTile(pos).IsSolid);
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
