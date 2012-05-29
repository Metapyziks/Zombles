using System;

using OpenTK;

using Zombles.Geometry;
using Zombles.Entities;

namespace Zombles.Scripts
{
    public class CorePlugin : Plugin
    {
        private Entity myEnt;

        protected override void OnInitialize()
        {
            Entity.Register( "human", delegate( Entity ent )
            {
                Render r = ent.AddComponent<Render>();
                r.Size = new Vector2( 0.5f, 1.0f );
                r.SetTexture( "human_stand_s" );
            } );

            Entity.Register( "zombie", delegate( Entity ent )
            {
                Render r = ent.AddComponent<Render>();
                r.Size = new Vector2( 0.5f, 1.0f );
                r.SetTexture( "zombie_stand_s" );
            } );
        }

        protected override void OnCityGenerated()
        {
            City city = ( Game.CurrentScene as GameScene ).City;
            Random rand = new Random();

            for ( int i = 0; i < 512; ++i )
            {
                myEnt = Entity.Create( "zombie", city );
                myEnt.Position = new Vector3( rand.NextSingle() * city.Width, 0.0f, rand.NextSingle() * city.Height );
                myEnt.Spawn();
            }
        }

        protected override void OnThink( double dt )
        {
            myEnt.GetComponent<Render>().TextureIndex = (ushort) ( (int) ( Game.Time * 4.0 ) % 4 + 4 );
        }
    }
}
