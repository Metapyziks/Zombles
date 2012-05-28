using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;

using Zombles.Graphics;
using Zombles.Entities;

namespace Zombles.Geometry
{
    public class District : IEnumerable<Block>
    {
        private List<Entity> myUnspawnedEnts;

        public readonly City City;

        public readonly Rectangle Bounds;
        public readonly int LongSide;
        public readonly int ShortSide;

        public int X { get { return Bounds.X; } }
        public int Y { get { return Bounds.Y; } }
        public int Width { get { return Bounds.Width; } }
        public int Height { get { return Bounds.Height; } }

        public bool IsRoot { get; private set; }
        public bool IsBranch { get; private set; }
        public bool IsHorzSplit { get; private set; }
        public bool IsLeaf { get; private set; }

        public District Parent { get; private set; }

        public District ChildA { get; private set; }
        public District ChildB { get; private set; }

        public Block Block { get; private set; }

        public District( City city, int x, int y, int width, int height )
            : this( (District) null, x, y, width, height )
        {
            City = city;
        }

        private District( District parent, int x, int y, int width, int height )
        {
            if( parent != null )
                City = parent.City;

            Bounds = new Rectangle( x, y, width, height );
            LongSide = Math.Max( Width, Height );
            ShortSide = Math.Min( Width, Height );

            IsRoot = parent == null;
            IsBranch = false;
            IsLeaf = false;

            Parent = parent;

            myUnspawnedEnts = new List<Entity>();
        }

        public void PlaceEntity( Entity ent )
        {
            myUnspawnedEnts.Add( ent );
        }

        public Block GetBlock( float x, float y )
        {
            if ( IsLeaf )
                return Block;

            if ( IsBranch )
            {
                if ( IsHorzSplit )
                {
                    if ( y >= ChildB.Y )
                        return ChildB.GetBlock( x, y );
                    return ChildA.GetBlock( x, y );
                }
                else
                {
                    if ( x >= ChildB.X )
                        return ChildB.GetBlock( x, y );
                    return ChildA.GetBlock( x, y );
                }
            }

            return null;
        }

        public void Split( bool horizontal, int offset )
        {
            if ( offset < 1 )
                throw new ArgumentOutOfRangeException( "Cannot split with an offset less than 1." );

            IsBranch = true;
            IsLeaf = false;
            IsHorzSplit = horizontal;
            if ( horizontal )
            {
                if ( offset > Height - 1 )
                    throw new ArgumentOutOfRangeException( "Cannot split with an offset greater than "
                        + ( Height - 1 ) + "." );

                ChildA = new District( this, X, Y, Width, offset );
                ChildB = new District( this, X, Y + offset, Width, Height - offset );
            }
            else
            {
                if ( offset > Width - 1 )
                    throw new ArgumentOutOfRangeException( "Cannot split with an offset greater than "
                        + ( Width - 1 ) + "." );

                ChildA = new District( this, X, Y, offset, Height );
                ChildB = new District( this, X + offset, Y, Width - offset, Height );
            }
        }

        public void SetBlock( Block block )
        {
            IsLeaf = true;
            Block = block;

            foreach ( Entity ent in myUnspawnedEnts )
                ent.Spawn();

            myUnspawnedEnts.Clear();
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

        public void RenderGeometry( VertexBuffer vb, GeometryShader shader, bool baseOnly = false )
        {
            if ( IsBranch )
            {
                if( ChildA.Bounds.IntersectsWith( shader.Camera.ViewBounds ) )
                    ChildA.RenderGeometry( vb, shader, baseOnly );
                if ( ChildB.Bounds.IntersectsWith( shader.Camera.ViewBounds ) )
                    ChildB.RenderGeometry( vb, shader, baseOnly );
            }
            else if ( IsLeaf )
                Block.RenderGeometry( vb, shader, baseOnly );
        }

        public void RenderEntities( FlatEntityShader shader, bool baseOnly = false )
        {
            if ( IsBranch )
            {
                if ( ChildA.Bounds.IntersectsWith( shader.Camera.ViewBounds ) )
                    ChildA.RenderEntities( shader );
                if ( ChildB.Bounds.IntersectsWith( shader.Camera.ViewBounds ) )
                    ChildB.RenderEntities( shader );
            }
            else if ( IsLeaf )
                Block.RenderEntities( shader );
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
