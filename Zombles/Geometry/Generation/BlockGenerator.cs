﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public abstract class BlockGenerator
    {
        public readonly int MinWidth;
        public readonly int MinHeight;

        protected BlockGenerator( int minWidth, int minHeight )
        {
            MinWidth = minWidth;
            MinHeight = minHeight;
        }

        public Block Generate( int x, int y, int width, int height, int seed = 0 )
        {
            Random rand = ( seed == 0 ? new Random() : new Random( seed ) );
            return Generate( x, y, width, height, rand );
        }

        public Block Generate( int x, int y, int width, int height, Random rand )
        {
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

            Block block = new Block( x, y, width, height );
            block.BuildTiles( tiles );
            return block;
        }

        protected abstract void Generate( int width, int height, TileBuilder[ , ] tiles, Random rand );
    }
}