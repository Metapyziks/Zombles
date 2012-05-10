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

        protected override void Generate( TileBuilder[ , ] tiles, int width, int height,
            int borderLeft, int borderTop,
            int borderRight, int borderBottom, Random rand )
        {
            GenHelper.BuildFloor( tiles, borderLeft, borderTop,
                width - borderLeft - borderRight,
                height - borderTop - borderBottom,
                0, "floor_concrete_0" );

            myBuildingGen.Generate( tiles, borderLeft, borderTop,
                width - borderLeft - borderRight,
                height - borderTop - borderBottom, rand );
        }
    }
}
