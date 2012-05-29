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
                RenderAnim r = ent.AddComponent<RenderAnim>();
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
                Entity ent = Entity.Create( "zombie", city );
                ent.Position = new Vector3( rand.NextSingle() * city.Width, 0.0f, rand.NextSingle() * city.Height );
                ent.GetComponent<RenderAnim>().Rotation = ( rand.NextSingle() - 0.5f ) * MathHelper.TwoPi;
                ent.Spawn();
            }
        }
    }
}
