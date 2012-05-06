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
            int rheight = rand.Next( 6 ) + 3;

            for ( int tx = x; tx < x + width; ++tx )
            {
                for ( int ty = y; ty < y + height; ++ty )
                {
                    tiles[ tx, ty ].SetRoof( rheight, "floor_roof_0" );

                    if ( tx == x )
                    {
                        tiles[ tx, ty ].SetWallRange( Face.West, 0, rheight, "wall_brick_0" );
                        tiles[ tx - 1, ty ].SetWallRange( Face.East, 0, rheight, "wall_brick_0" );
                    }

                    if ( ty == y )
                    {
                        tiles[ tx, ty ].SetWallRange( Face.North, 0, rheight, "wall_brick_0" );
                        tiles[ tx, ty - 1 ].SetWallRange( Face.South, 0, rheight, "wall_brick_0" );
                    }

                    if ( tx == x + width - 1 )
                    {
                        tiles[ tx, ty ].SetWallRange( Face.East, 0, rheight, "wall_brick_0" );
                        tiles[ tx + 1, ty ].SetWallRange( Face.West, 0, rheight, "wall_brick_0" );
                    }

                    if ( ty == y + height - 1 )
                    {
                        tiles[ tx, ty ].SetWallRange( Face.South, 0, rheight, "wall_brick_0" );
                        tiles[ tx, ty + 1 ].SetWallRange( Face.North, 0, rheight, "wall_brick_0" );
                    }
                }
            }
        }
    }
}
