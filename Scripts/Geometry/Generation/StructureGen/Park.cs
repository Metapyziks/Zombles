using System;

using Zombles;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.StructureGen
{
    public class Park : StructureGenerator
    {
        public bool Fence;

        public override void Generate( TileBuilder[ , ] tiles, int x, int y, int width, int height, Random rand )
        {
            Func <int, int, Face, bool, string> wallFunc =
                delegate( int horzPos, int level, Face face, bool isInterior )
                {
                    if ( level == 0 )
                        return "wall_brick_0";
                    return "wall_fence_0";
                };
            Func <int, int, string> floorFunc =
                delegate( int horzPos, int vertPos )
                {
                    return rand.NextTexture( "floor_grass_", 4 );
                };
            GenHelper.BuildFloor( tiles, x, y, width, height, 0, floorFunc );
            GenHelper.BuildWall( tiles, x, y, Face.North, width, 2, wallFunc );
            GenHelper.BuildWall( tiles, x + width - 1, y, Face.East, height, 2, wallFunc );
            GenHelper.BuildWall( tiles, x, y + height - 1, Face.South, width, 2, wallFunc );
            GenHelper.BuildWall( tiles, x, y, Face.West, height, 2, wallFunc );
        }
    }
}
