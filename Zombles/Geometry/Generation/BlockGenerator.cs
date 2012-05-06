using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public class BlockGenerator
    {
        public readonly int MinWidth;
        public readonly int MinHeight;

        public BlockGenerator()
            : this( 4, 4 )
        {

        }

        protected BlockGenerator( int minWidth, int minHeight )
        {
            MinWidth = minWidth;
            MinHeight = minHeight;
        }

        public Block Generate( int x, int y, int width, int height, int seed = 0 )
        {
            Random rand = ( seed == 0 ? new Random() : new Random( seed ) );

            TileBuilder[,] tiles = new TileBuilder[ width, height ];

            for ( int tx = 0; tx < width; ++tx ) for ( int ty = 0; ty < height; ++ty )
            {
                if ( tx >= 2 && ty >= 2 && tx < width - 2 && ty < height - 2 )
                    tiles[ tx, ty ] = new TileBuilder();
                else
                {
                    TileBuilder b = tiles[ tx, ty ] = new TileBuilder();

                    if ( tx >= 1 && ty >= 1 && tx < width - 1 && ty < height - 1 )
                    {
                        if ( tx == 1 )
                        {
                            if ( ty == 1 )
                                b.SetFloor( "floor_pavement_b" );
                            else if ( ty == height - 2 )
                                b.SetFloor( "floor_pavement_f" );
                            else
                            {
                                if ( ty % 8 == 4 )
                                    b.SetFloor( "floor_pavement_e" );
                                else
                                    b.SetFloor( rand.NextTexture( "floor_pavement_", 12, 14 ) );
                            }
                        }
                        else if ( tx == width - 2 )
                        {
                            if ( ty == 1 )
                                b.SetFloor( "floor_pavement_7" );
                            else if ( ty == height - 2 )
                                b.SetFloor( "floor_pavement_3" );
                            else
                            {
                                if ( ty % 8 == 4 )
                                    b.SetFloor( "floor_pavement_6" );
                                else
                                    b.SetFloor( rand.NextTexture( "floor_pavement_", 4, 6 ) );
                            }
                        }
                        else
                        {
                            if ( ty == 1 )
                            {
                                if ( tx % 8 == 4 )
                                    b.SetFloor( "floor_pavement_a" );
                                else
                                    b.SetFloor( rand.NextTexture( "floor_pavement_", 8, 10 ) );
                            }
                            else if ( ty == height - 2 )
                            {
                                if ( tx % 8 == 4 )
                                    b.SetFloor( "floor_pavement_2" );
                                else
                                    b.SetFloor( rand.NextTexture( "floor_pavement_", 0, 2 ) );
                            }
                        }
                    }
                    else
                        b.SetFloor( rand.NextTexture( "floor_road_", 0, 4 ) );
                }
            }

            Generate( width, height, tiles, rand );

            for ( int tx = 0; tx < width; ++tx ) for ( int ty = 0; ty < height; ++ty )
            {
                TileBuilder b = tiles[ tx, ty ];
                if ( tx < width - 1 ) b.CullHiddenWalls( tiles[ tx + 1, ty ], Face.East );
                if ( ty < height - 1 ) b.CullHiddenWalls( tiles[ tx, ty + 1 ], Face.South );
            }

            Block block = new Block( x, y, width, height );
            block.BuildTiles( tiles );
            return block;
        }

        protected void Generate( int width, int height, TileBuilder[ , ] tiles, Random rand )
        {
            for ( int x = 2; x < width - 2; ++x ) for ( int y = 2; y < height - 2; ++y )
                tiles[ x, y ].SetFloor( "floor_concrete_0" );

            int crateCount = rand.Next( ( width - 2 ) * ( height - 2 ) / 8 );

            for ( int i = 0; i < crateCount; ++i )
            {
                int x = rand.Next( 2, width - 2 );
                int y = rand.Next( 2, height - 2 );

                tiles[ x, y ].SetFloor();
                tiles[ x, y ].SetRoof( 1, "floor_crate_0" );
                tiles[ x + 1, y ].SetWall( Face.West, 0, "wall_crate_0" );
                tiles[ x, y + 1 ].SetWall( Face.North, 0, "wall_crate_0" );
                tiles[ x - 1, y ].SetWall( Face.East, 0, "wall_crate_0" );
                tiles[ x, y - 1 ].SetWall( Face.South, 0, "wall_crate_0" );
            }
        }
    }
}
