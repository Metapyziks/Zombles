using System;

using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.BlockGen
{
    public class Warehouse : BlockGenerator
    {
        private StructureGen.Warehouse myBuildingGen;

        public Warehouse()
            : base( 1.0, 8, 12, 24, 32 )
        {
            myBuildingGen = new StructureGen.Warehouse();
        }

        protected override void Generate( District district, TileBuilder[ , ] tiles,
            int borderLeft, int borderTop,
            int borderRight, int borderBottom, Random rand )
        {
            GenHelper.BuildFloor( tiles, borderLeft, borderTop,
                district.Width - borderLeft - borderRight,
                district.Height - borderTop - borderBottom,
                0, "floor_concrete_0" );

            myBuildingGen.EntranceFaces =
                ( borderLeft > 1 ? Face.West : Face.None ) |
                ( borderTop > 1 ? Face.North : Face.None ) |
                ( borderRight > 1 ? Face.East : Face.None ) |
                ( borderBottom > 1 ? Face.South : Face.None );

            myBuildingGen.Generate( district, tiles, borderLeft, borderTop,
                district.Width - borderLeft - borderRight,
                district.Height - borderTop - borderBottom, rand );
        }
    }
}
