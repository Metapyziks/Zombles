using System;
using ResourceLibrary;
using Zombles;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.StructureGen
{
    public class Park : StructureGenerator
    {
        public bool Fence;

        public override void Generate(District district, TileBuilder[,] tiles, int x, int y, int width, int height, Random rand)
        {
            Face entryFaces = Face.None;

            for (int i = 0; i < 4; ++i)
                entryFaces |= rand.NextFace();

            int fenceHeight = rand.NextDouble() < 0.8 ? 2 : 1;

            Func <int, int, Face, bool, ResourceLocator> wallFunc = (horzPos, level, face, isInterior) => {
                if ((entryFaces & face) != Face.None) {
                    if (face == Face.North || face == Face.South) {
                        if (Math.Abs(horzPos * 2 - width + 1) < 3)
                            return "";
                    } else {
                        if (Math.Abs(horzPos * 2 - height + 1) < 3)
                            return "";
                    }
                }

                if (level < fenceHeight - 1)
                    return "wall/brick0/0";
                return "wall/fence/0";
            };
            Func <int, int, ResourceLocator> floorFunc = (horzPos, vertPos) => {
                return rand.NextTexture("floor/grass", 4);
            };
            GenHelper.BuildFloor(tiles, x, y, width, height, 0, floorFunc);

            if (Fence) {
                GenHelper.BuildWall(tiles, x, y, Face.North, width, fenceHeight, wallFunc);
                GenHelper.BuildWall(tiles, x + width - 1, y, Face.East, height, fenceHeight, wallFunc);
                GenHelper.BuildWall(tiles, x, y + height - 1, Face.South, width, fenceHeight, wallFunc);
                GenHelper.BuildWall(tiles, x, y, Face.West, height, fenceHeight, wallFunc);
            }
        }
    }
}
