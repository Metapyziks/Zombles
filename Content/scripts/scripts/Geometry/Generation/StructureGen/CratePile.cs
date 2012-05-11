using System;

using Zombles;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.StructureGen
{
    public class CratePile : StructureGenerator
    {
        public int MaxHeight { get; set; }

        public CratePile( int maxHeight = 2 )
        {
            MaxHeight = maxHeight;
        }

        public override void Generate( TileBuilder[ , ] tiles, int x, int y, int width, int height, Random rand )
        {
            for ( int tx = 0; tx < width; ++tx )
            {
                for ( int ty = 0; ty < height; ++ty )
                {
                    if ( rand.Next( 4 ) < 2 )
                    {
                        GenHelper.BuildSolid( tiles, x + tx, y + ty, 1, 1, rand.Next( 1, MaxHeight + 1 ),
                            "wall_crate_0", "floor_crate_0" );
                    }
                }
            }
        }
    }
}
