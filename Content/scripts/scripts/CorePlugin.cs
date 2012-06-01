using System;

using OpenTK;

using Zombles.Graphics;
using Zombles.Geometry;
using Zombles.Entities;

using Zombles.Scripts.Entities;

namespace Zombles.Scripts
{
    public class CorePlugin : Plugin
    {
        protected override void OnInitialize()
        {
            Entity.Register( "human", delegate( Entity ent )
            {
                ent.AddComponent<RenderAnim>().Size = new Vector2( 0.5f, 1.0f );
                ent.AddComponent<Collision>().SetDimentions( 0.5f, 0.5f ).Model = CollisionModel.Repel;
                ent.AddComponent<Movement>();
                ent.AddComponent<Health>();
            } );

            Entity.Register( "survivor", "human", delegate( Entity ent )
            {
                ent.AddComponent<Survivor>();
                ent.AddComponent<SurvivorAI>();
            } );

            Entity.Register( "zombie", "human", delegate( Entity ent )
            {
                ent.AddComponent<Zombie>();
                ent.AddComponent<ZombieAI>();
            } );
        }

        protected override void OnCityGenerated()
        {
            City city = ( ZomblesGame.CurrentScene as GameScene ).City;
            Random rand = new Random();

            for ( int i = 0; i < 992; ++i )
            {
                Entity surv = Entity.Create( "survivor", city );
                surv.Position = new Vector3( rand.NextSingle() * city.Width, 0.0f, rand.NextSingle() * city.Height );
                surv.GetComponent<RenderAnim>().Rotation = ( rand.NextSingle() - 0.5f ) * MathHelper.TwoPi;
                surv.Spawn();
            }

            for ( int i = 0; i < 32; ++i )
            {
                Entity zomb = Entity.Create( "zombie", city );
                zomb.Position = new Vector3( rand.NextSingle() * city.Width, 0.0f, rand.NextSingle() * city.Height );
                zomb.GetComponent<RenderAnim>().Rotation = ( rand.NextSingle() - 0.5f ) * MathHelper.TwoPi;
                zomb.Spawn();
            }
        }
    }
}
