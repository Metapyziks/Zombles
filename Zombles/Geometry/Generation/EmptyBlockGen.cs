using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation
{
    public class EmptyBlockGen : BlockGenerator
    {
        public EmptyBlockGen()
            : base( 0, 256 )
        {

        }

        protected override void Generate( int width, int height, TileBuilder[ , ] tiles, Random rand )
        {
            for ( int x = 2; x < width - 2; ++x ) for ( int y = 2; y < height - 2; ++y )
                    tiles[ x, y ].SetFloor( "floor_concrete_0" );
        }
    }
}
