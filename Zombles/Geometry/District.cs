using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zombles.Graphics;

namespace Zombles.Geometry
{
    public class District
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;
        public readonly int LongSide;
        public readonly int ShortSide;

        public bool IsBranch { get; private set; }
        public bool IsLeaf { get; private set; }

        public District ChildA { get; private set; }
        public District ChildB { get; private set; }

        public Block Block { get; private set; }

        public District( int x, int y, int width, int height )
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            LongSide = Math.Max( Width, Height );
            ShortSide = Math.Min( Width, Height );

            IsBranch = false;
            IsLeaf = false;
        }

        public void Split( bool horizontal, int offset )
        {
            IsBranch = true;
            IsLeaf = false;
            if ( horizontal )
            {
                ChildA = new District( X, Y, Width, offset );
                ChildB = new District( X, Y + offset, Width, Height - offset );
            }
            else
            {
                ChildA = new District( X, Y, offset, Height );
                ChildB = new District( X + offset, Y, Width - offset, Height );
            }
        }

        public void SetBlock( Block block )
        {
            IsLeaf = true;
            Block = block;
        }

        public void Render( GeometryShader shader, bool baseOnly = false )
        {
            if ( IsBranch )
            {
                ChildA.Render( shader, baseOnly );
                ChildB.Render( shader, baseOnly );
            }
            else if ( IsLeaf )
                Block.Render( shader, baseOnly );
        }
    }
}
