using System;

using Zombles;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.StructureGen
{
    public class Warehouse : StructureGenerator
    {
        public Warehouse()
            : base( 4, 4 )
        {

        }

        public override void Generate( TileBuilder[ , ] tiles, int x, int y, int width, int height, Random rand )
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

                GenHelper.BuildSolid( tiles, tx, ty, 1, 1, 1, "wall_crate_0", "floor_crate_0" );
            }

            Func<int,int,bool,String> texFunc = delegate( int horzpos, int level, bool isInterior )
            {
                if( level < rheight )
                    return rand.NextTexture( "wall_brick_", 4 );
                return "wall_brick_7";
            };

            GenHelper.BuildWall( tiles, x, y, Face.North, width, rheight + 1, texFunc );
            GenHelper.BuildWall( tiles, x, y, Face.West, height, rheight + 1, texFunc );
            GenHelper.BuildWall( tiles, x, y + height - 1, Face.South, width, rheight + 1, texFunc );
            GenHelper.BuildWall( tiles, x + width - 1, y, Face.East, height, rheight + 1, texFunc );
        }
    }
}
