using System;

using Zombles;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.BuildingGen
{
    public class Warehouse : BuildingGenerator
    {
        public Warehouse()
            : base( 4, 4 )
        {

        }

        public override void Generate( int x, int y, int width, int height, TileBuilder[ , ] tiles, Random rand )
        {
            int rheight = rand.Next( 5 ) + 3;

            for ( int tx = x; tx < x + width; ++tx )
                for ( int ty = y; ty < y + height; ++ty )
                    tiles[ tx, ty ].SetRoof( rheight, "floor_roof_0" );

            int crateCount = rand.Next( ( width - 2 ) * ( height - 2 ) / 8 );
            
            for ( int i = 0; i < crateCount; ++i )
            {
                int tx = rand.Next( x, x + width );
                int ty = rand.Next( y, y + height );

                tiles[ tx, ty ].SetFloor( 1, "floor_crate_0" );
                tiles[ tx + 1, ty ].SetWall( Face.West, 0, "wall_crate_0" );
                tiles[ tx, ty + 1 ].SetWall( Face.North, 0, "wall_crate_0" );
                tiles[ tx - 1, ty ].SetWall( Face.East, 0, "wall_crate_0" );
                tiles[ tx, ty - 1 ].SetWall( Face.South, 0, "wall_crate_0" );
            }

            Func<int,int,bool,String> texFunc = delegate( int horzpos, int level, bool isInterior )
            {
                if( level < rheight )
                    return rand.NextTexture( "wall_brick_", 4 );
                return "wall_brick_7";
            };

            BuildWall( x, y, Face.North, width, rheight + 1, texFunc, tiles );
            BuildWall( x, y, Face.West, height, rheight + 1, texFunc, tiles );
            BuildWall( x, y + height - 1, Face.South, width, rheight + 1, texFunc, tiles );
            BuildWall( x + width - 1, y, Face.East, height, rheight + 1, texFunc, tiles );
        }
    }
}
