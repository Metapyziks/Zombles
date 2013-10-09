using System;

using Zombles;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.Structures
{
    public class CratePile : StructureGenerator
    {
        public int MaxHeight { get; set; }
        public double CrateFrequency { get; set; }

        public CratePile()
        {
            MaxHeight = 2;
            CrateFrequency = 0.2;
        }

        public override void Generate(District district, TileBuilder[,] tiles, Random rand)
        {
            for (int x = 0; x < SizeX; ++x) {
                for (int y = 0; y < SizeY; ++y) {
                    if (rand.NextDouble() <= CrateFrequency) {
                        GenHelper.BuildSolid(tiles, X + x, Y + y, 1, 1, rand.Next(1, MaxHeight + 1),
                            "wall/crate/0", "floor/crate/0");
                    }
                }
            }
        }
    }
}
