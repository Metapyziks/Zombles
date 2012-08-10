using System;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.BlockGen
{
    class Church : BlockGenerator
    {
        private StructureGen.Church myBuildingGen;

        public Church()
            : base(1.0, 10, 14, 24, 32)
        {

            myBuildingGen = new StructureGen.Church();

        }

        protected override void Generate(District district, TileBuilder[,] tiles,
            int borderLeft, int borderTop,
            int borderRight, int borderBottom, Random rand)
        {
            GenHelper.BuildFloor(tiles, borderLeft, borderTop,
                district.Width - borderLeft - borderRight,
                district.Height - borderTop - borderBottom,
                0, "floor_church_1");

          /*  myBuildingGen.EntranceFace =
               (borderLeft > 1 ? Face.West : Face.None) |
               (borderTop > 1 ? Face.North : Face.None) |
               (borderRight > 1 ? Face.East : Face.None) |
               (borderBottom > 1 ? Face.South : Face.None);

            myBuildingGen.Generate(district, tiles, borderLeft, borderTop,
                district.Width - borderLeft - borderRight,
                district.Height - borderTop - borderBottom, rand); */



        }

    }
}
