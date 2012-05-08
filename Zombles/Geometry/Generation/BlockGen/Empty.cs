﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Geometry.Generation.BlockGen
{
    public class Empty : BlockGenerator
    {
        public Empty()
            : base( 0, 256 )
        {

        }

        protected override void Generate( int width, int height, int borderLeft, int borderTop,
            int borderRight, int borderBottom, TileBuilder[ , ] tiles, Random rand )
        {
            BuildFloor( borderLeft, borderTop,
                width - borderLeft - borderRight,
                height - borderTop - borderBottom,
                0, "floor_concrete_0", tiles );
        }
    }
}