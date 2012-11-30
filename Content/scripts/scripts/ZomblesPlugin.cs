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
            Entity.Register( "waypoint", ent =>
            {
                ent.AddComponent<Waypoint>();
                // ent.AddComponent<Render2D>().TextureIndex = TextureManager.Ents.GetIndex( "waypoint" );
            } );

            Entity.Register( "human", ent =>
            {
                ent.AddComponent<RenderAnim>();
                ent.AddComponent<Collision>().SetDimentions( 0.5f, 0.5f ).Model = CollisionModel.Repel | CollisionModel.Entity;
                ent.AddComponent<Movement>();
                ent.AddComponent<PathNavigation>();
                ent.AddComponent<Health>();
            } );

            Entity.Register( "survivor", "human", ent =>
            {
                ent.AddComponent<Survivor>();
                ent.AddComponent<RTSControl>();
            } );

            Entity.Register( "zombie", "human", ent =>
            {
                ent.AddComponent<Zombie>();
                ent.AddComponent<ZombieAI>();
            } );
        }

        protected override void OnCityGenerated()
        {
            GameScene scene = ZomblesGame.CurrentScene as GameScene;
            City city = scene.City;
            Random rand = Tools.Random;

            Waypoint.GenerateNetwork( city );

            int count = ( city.Width * city.Height ) / 64;
            int zoms = Math.Max( count / 32, 8 );

            System.Diagnostics.Debugger.Break();

            for ( int i = 0; i < count - zoms; ++i )
            {
                Entity surv = Entity.Create( city, "survivor" );
                surv.Position = new Vector3( rand.NextSingle() * city.Width, 0.0f, rand.NextSingle() * city.Height );
                surv.Spawn();

                scene.SelectedEntities.Add( surv );
            }

            for ( int i = 0; i < zoms; ++i )
            {
                Entity zomb = Entity.Create( city, "zombie" );
                zomb.Position = new Vector3( rand.NextSingle() * city.Width, 0.0f, rand.NextSingle() * city.Height );
                zomb.Spawn();
            }
        }
    }
}
