using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public class TestBuildingGen : BuildingGenerator
    {
        public TestBuildingGen()
            : base( 4, 4 )
        {

        }

        public override void Generate( int x, int y, int width, int height, TileBuilder[ , ] tiles, Random rand )
        {
            int rheight = rand.Next( 5 ) + 3;

            for ( int tx = x; tx < x + width; ++tx )
                for ( int ty = y; ty < y + height; ++ty )
                    tiles[ tx, ty ].SetRoof( rheight, "floor_roof_0" );

            Func<int,bool,String> texFunc = delegate( int level, bool isInterior )
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
