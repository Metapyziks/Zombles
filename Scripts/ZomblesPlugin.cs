using System;

using OpenTK;

using Zombles.Graphics;
using Zombles.Geometry;
using Zombles.Entities;

using Zombles.Scripts.Entities;

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
                    .Model = CollisionModel.Repel | CollisionModel.Entity;
                ent.AddComponent<Movement>();
                ent.AddComponent<Health>();
            });

            Entity.Register("survivor", "human", ent => {
                ent.AddComponent<Survivor>();
                ent.AddComponent<SurvivorAI>();
                ent.AddComponent<RouteNavigation>();
            });

            Entity.Register("zombie", "human", ent => {
                ent.AddComponent<Zombie>();
                ent.AddComponent<ZombieAI>();
            });

            MainWindow.SetScene(new GameScene(Game));
        }

        protected override void OnCityGenerated()
        {
            GameScene scene = MainWindow.CurrentScene as GameScene;
            City city = scene.City;
            Random rand = Tools.Random;

            int count = 256;
            int zoms = Math.Max(count / 32, 8);

            Func<Vector2> randPos = () => {
                Vector2 pos;
                do {
                    pos = new Vector2(rand.NextSingle() * city.Width, rand.NextSingle() * city.Height);
                } while (city.GetTile(pos).FloorHeight > 0);
                return pos;
            };

            for (int i = 0; i < count - zoms; ++i) {
                Entity surv = Entity.Create(city, "survivor");
                surv.Position2D = randPos();
                surv.Spawn();
            }

            for (int i = 0; i < zoms; ++i) {
                Entity zomb = Entity.Create(city, "zombie");
                zomb.Position2D = randPos();
                zomb.Spawn();
            }
        }
    }
}
