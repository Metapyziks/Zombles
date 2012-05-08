﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Zombles.Graphics;

namespace Zombles.Geometry
{
    public class District : IEnumerable<Block>
    {
        public readonly Rectangle Bounds;
        public readonly int LongSide;
        public readonly int ShortSide;

        public int X { get { return Bounds.X; } }
        public int Y { get { return Bounds.Y; } }
        public int Width { get { return Bounds.Width; } }
        public int Height { get { return Bounds.Height; } }

        public bool IsRoot { get; private set; }
        public bool IsBranch { get; private set; }
        public bool IsLeaf { get; private set; }

        public District Parent { get; private set; }

        public District ChildA { get; private set; }
        public District ChildB { get; private set; }

        public Block Block { get; private set; }

        public District( int x, int y, int width, int height )
            : this( null, x, y, width, height ) { }

        private District( District parent, int x, int y, int width, int height )
        {
            Bounds = new Rectangle( x, y, width, height );
            LongSide = Math.Max( Width, Height );
            ShortSide = Math.Min( Width, Height );

            IsRoot = parent == null;
            IsBranch = false;
            IsLeaf = false;

            Parent = parent;
        }

        public void Split( bool horizontal, int offset )
        {
            IsBranch = true;
            IsLeaf = false;
            if ( horizontal )
            {
                ChildA = new District( this, X, Y, Width, offset );
                ChildB = new District( this, X, Y + offset, Width, Height - offset );
            }
            else
            {
                ChildA = new District( this, X, Y, offset, Height );
                ChildB = new District( this, X + offset, Y, Width - offset, Height );
            }
        }

        public void SetBlock( Block block )
        {
            IsLeaf = true;
            Block = block;
        }

        public int GetVertexCount()
        {
            if ( IsBranch )
                return ChildA.GetVertexCount() + ChildB.GetVertexCount();
            else if ( IsLeaf )
                return Block.GetVertexCount();

            return 0;
        }

        public void GetVertices( float[] verts, ref int i )
        {
            if ( IsBranch )
            {
                ChildA.GetVertices( verts, ref i );
                ChildB.GetVertices( verts, ref i );
            }
            else if ( IsLeaf )
                Block.GetVertices( verts, ref i );
        }

        public void Render( VertexBuffer vb, GeometryShader shader, bool baseOnly = false )
        {
            if ( IsBranch )
            {
                if( ChildA.Bounds.IntersectsWith( shader.ViewBounds ) )
                    ChildA.Render( vb, shader, baseOnly );
                if ( ChildB.Bounds.IntersectsWith( shader.ViewBounds ) )
                    ChildB.Render( vb, shader, baseOnly );
            }
            else if ( IsLeaf )
                Block.Render( vb, shader, baseOnly );
        }

        public IEnumerator<Block> GetEnumerator()
        {
            return new DistrictEnumerator( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
