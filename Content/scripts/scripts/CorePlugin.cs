using System;

using OpenTK;

using Zombles.Graphics;
using Zombles.Geometry;
using Zombles.Entities;

namespace Zombles.Scripts
{
    public class CorePlugin : Plugin
    {
        protected override void OnInitialize()
        {
            Entity.Register( "human", delegate( Entity ent )
            {
                ent.SetComponent<RenderAnim>().Size = new Vector2( 0.5f, 1.0f );
                ent.SetComponent<Collision>().SetDimentions( 0.5f, 0.5f ).Model = CollisionModel.Repel;
                ent.SetComponent<Movement>();
                ent.SetComponent<Control>();
            } );

            Entity.Register( "survivor", "human", delegate( Entity ent )
            {
                RenderAnim r = ent.GetComponent<RenderAnim>();
                r.Start( EntityAnim.GetAnim( "human walk" ) );
            } );

            Entity.Register( "zombie", "human", delegate( Entity ent )
            {
                RenderAnim r = ent.GetComponent<RenderAnim>();
                r.Start( EntityAnim.GetAnim( "zombie walk" ) );
            } );
        }

        protected override void OnCityGenerated()
        {
            City city = ( ZomblesGame.CurrentScene as GameScene ).City;
            Random rand = new Random();

            for ( int i = 0; i < 1024; ++i )
            {
                Entity zomb = Entity.Create( "zombie", city );
                zomb.Position = new Vector3( rand.NextSingle() * city.Width, 0.0f, rand.NextSingle() * city.Height );
                zomb.GetComponent<RenderAnim>().Rotation = ( rand.NextSingle() - 0.5f ) * MathHelper.TwoPi;
                zomb.Spawn();
            }

            Entity surv = Entity.Create( "survivor", city );
            surv.Position = new Vector3( city.Width / 2.0f, 0.0f, city.Height / 2.0f );
            surv.GetComponent<RenderAnim>().Rotation = ( rand.NextSingle() - 0.5f ) * MathHelper.TwoPi;
            surv.Spawn();
        }
    }
}
