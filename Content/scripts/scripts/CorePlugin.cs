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
                r.Anim = EntityAnim.GetAnim( "human stand" ); 
            } );

            Entity.Register( "zombie", "human", delegate( Entity ent )
            {
                RenderAnim r = ent.GetComponent<RenderAnim>();
                r.Anim = EntityAnim.GetAnim( "zombie stand" ); 
            } );
        }

        protected override void OnCityGenerated()
        {
            City city = ( Game.CurrentScene as GameScene ).City;
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
