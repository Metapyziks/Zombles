using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public class WarehouseBlockGen : BlockGenerator
    {
        private WarehouseBuildingGen myBuildingGen;

        public WarehouseBlockGen()
            : base( 8, 12, 24, 32 )
        {
            myBuildingGen = new WarehouseBuildingGen();
        }

        protected override void Generate( int width, int height, TileBuilder[ , ] tiles, Random rand )
        {
            for ( int x = 2; x < width - 2; ++x ) for ( int y = 2; y < height - 2; ++y )
                    tiles[ x, y ].SetFloor( "floor_concrete_0" );

            myBuildingGen.Generate( 2, 2, width - 4, height - 4, tiles, rand );
        }
    }
}
