using System;

using OpenTK;

using Zombles;
using Zombles.Geometry;
using Zombles.Geometry.Generation;
using Zombles.Entities;

namespace Zombles.Scripts.Geometry.Generation.StructureGen
{
    public class Warehouse : StructureGenerator
    {
        private CratePile myCrateGen;

        public Face EntranceFaces { get; set; }

        public Warehouse( Face entranceFaces = Face.All )
        {
            myCrateGen = new CratePile();
            myCrateGen.MaxHeight = 2;

            EntranceFaces = entranceFaces;
        }

        public override void Generate( District district, TileBuilder[ , ] tiles, int x, int y, int width, int height, Random rand )
        {
            if ( EntranceFaces != Face.None )
            {
                int rheight = rand.Next( 3 ) + 4;

                GenHelper.BuildRoof( tiles, x, y, width, height, rheight, "floor_roof_0" );

                Func<int,int,Face,bool,String> texFunc = delegate( int horzpos, int level, Face face, bool isInterior )
                {
                    if ( level < rheight )
                        return rand.NextTexture( "wall_brick_", 4 );
                    return "wall_brick_7";
                };

                GenHelper.BuildWall( tiles, x, y, Face.North, width, rheight + 1, texFunc );
                GenHelper.BuildWall( tiles, x, y, Face.West, height, rheight + 1, texFunc );
                GenHelper.BuildWall( tiles, x, y + height - 1, Face.South, width, rheight + 1, texFunc );
                GenHelper.BuildWall( tiles, x + width - 1, y, Face.East, height, rheight + 1, texFunc );

                Face entrance = rand.NextFace( EntranceFaces );
                int entranceSize = rand.Next( 2, 4 );
                if ( ( (int) entrance & 0x5 ) != 0 )
                {
                    int entranceCount = Math.Min( ( height - 1 ) / ( entranceSize + 1 ), 3 );
                    int entranceOffset = rand.Next( 1, height - ( entranceSize + 1 ) * entranceCount );
                    int entranceX = entrance == Face.West ? x : x + width;
                    int entranceY = y + entranceOffset;
                    GenHelper.BuildWall( tiles, entranceX, entranceY - 1, Face.West, 1, 3,
                        "wall_brick_9", "wall_brick_8" );
                    for ( int i = 0; i < entranceCount; ++i )
                    {
                        int doorOffset = rand.Next( 0, 2 ) * 2;
                        int doorHeight = 3 - doorOffset;
                        GenHelper.BuildWall( tiles, entranceX, entranceY + i * ( entranceSize + 1 ),
                            Face.West, entranceSize, doorOffset, null, null );
                        GenHelper.BuildWall( tiles, entranceX, entranceY + i * ( entranceSize + 1 ),
                            Face.West, entranceSize, doorOffset, doorHeight, "wall_garage_0", "wall_garage_0" );
                        GenHelper.BuildWall( tiles, entranceX, entranceY + i * ( entranceSize + 1 ) + entranceSize,
                            Face.West, 1, 3, "wall_brick_a", "wall_brick_a" );
                    }
                    GenHelper.BuildWall( tiles, entranceX,
                        entranceY + entranceCount * ( entranceSize + 1 ) - 1, Face.West, 1, 3,
                        "wall_brick_8", "wall_brick_9" );
                }
                else
                {
                    int entranceCount = Math.Min( ( width - 1 ) / ( entranceSize + 1 ), 3 );
                    int entranceOffset = rand.Next( 1, width - ( entranceSize + 1 ) * entranceCount );
                    int entranceX = x + entranceOffset;
                    int entranceY = entrance == Face.North ? y : y + height;
                    GenHelper.BuildWall( tiles, entranceX - 1, entranceY, Face.North, 1, 3,
                        "wall_brick_8", "wall_brick_9" );
                    for ( int i = 0; i < entranceCount; ++i )
                    {
                        int doorOffset = rand.Next( 0, 2 ) * 2;
                        int doorHeight = 3 - doorOffset;
                        GenHelper.BuildWall( tiles, entranceX + i * ( entranceSize + 1 ), entranceY,
                            Face.North, entranceSize, doorOffset, null, null );
                        GenHelper.BuildWall( tiles, entranceX + i * ( entranceSize + 1 ), entranceY,
                            Face.North, entranceSize, doorOffset, doorHeight, "wall_garage_0", "wall_garage_0" );
                        GenHelper.BuildWall( tiles, entranceX + i * ( entranceSize + 1 ) + entranceSize, entranceY,
                            Face.North, 1, 3, "wall_brick_a", "wall_brick_a" );
                    }
                    GenHelper.BuildWall( tiles, entranceX + entranceCount * ( entranceSize + 1 ) - 1,
                        entranceY, Face.North, 1, 3,
                        "wall_brick_9", "wall_brick_8" );
                }
            }
        }
    }
}
