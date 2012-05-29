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
                RenderAnim r = ent.SetComponent<RenderAnim>();
                r.Size = new Vector2( 0.5f, 1.0f );
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

            for ( int i = 0; i < 4096; ++i )
            {
                Entity ent = Entity.Create( "survivor", city );
                ent.Position = new Vector3( rand.NextSingle() * city.Width, 0.0f, rand.NextSingle() * city.Height );
                ent.GetComponent<RenderAnim>().Rotation = ( rand.NextSingle() - 0.5f ) * MathHelper.TwoPi;
                ent.Spawn();
            }

            NearbyEntityEnumerator iter = new NearbyEntityEnumerator( city, new Vector2( city.Width / 2.0f, city.Height / 2.0f ), 32.0f );
            while ( iter.MoveNext() )
            {
                Vector3 pos = iter.Current.Position;
                pos.Y += 8.0f;
                iter.Current.Position = pos;
            }
        }
    }
}
