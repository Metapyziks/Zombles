using System;

using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Scripts.Geometry.Generation.BlockGen
{
    public class Warehouse : BlockGenerator
    {
        private BuildingGen.Warehouse myBuildingGen;

        public Warehouse()
            : base( 1.0, 8, 12, 24, 32 )
        {
            myBuildingGen = new BuildingGen.Warehouse();
        }

        protected override void Generate( int width, int height, int borderLeft, int borderTop,
            int borderRight, int borderBottom, TileBuilder[ , ] tiles, Random rand )
        {
            BuildFloor( borderLeft, borderTop,
                width - borderLeft - borderRight,
                height - borderTop - borderBottom,
                0, "floor_concrete_0", tiles );

            myBuildingGen.Generate( borderLeft, borderTop,
                width - borderLeft - borderRight,
                height - borderTop - borderBottom,
                tiles, rand );
        }
    }
}
