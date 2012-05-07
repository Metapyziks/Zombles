using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public class TestBlockGen : BlockGenerator
    {
        private TestBuildingGen myBuildingGen;

        public TestBlockGen()
            : base( 10, 16, 16, 24 )
        {
            myBuildingGen = new TestBuildingGen();
        }

        protected override void Generate( int width, int height, TileBuilder[ , ] tiles, Random rand )
        {
            for ( int x = 2; x < width - 2; ++x ) for ( int y = 2; y < height - 2; ++y )
                    tiles[ x, y ].SetFloor( "floor_concrete_0" );

            myBuildingGen.Generate( 3, 3, width - 6, height - 6, tiles, rand );
        }
    }
}
