using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Graphics;
using Zombles.Entities;

namespace Zombles.Geometry
{
    public class City : IEnumerable<Block>, IDisposable
    {
        private VertexBuffer myGeomVertexBuffer;
        private VertexBuffer myPathVertexBuffer;
        private LumTexture2D myBloodMap;

        private bool mySetPathVB;

        public District RootDistrict { get; private set; }

        public int Width { get { return RootDistrict.Width; } }
        public int Height { get { return RootDistrict.Height; } }

        public int Depth { get { return RootDistrict.Depth; } }

        public City( int width, int height )
        {
            RootDistrict = new District( this, 0, 0, width, height );
            myGeomVertexBuffer = new VertexBuffer( 3 );
            myPathVertexBuffer = new VertexBuffer( 2 );
            myBloodMap = new LumTexture2D( width * 2, height * 2 );

            mySetPathVB = false;
        }

        public Vector2 Difference( Vector2 a, Vector2 b )
        {
            Vector2 diff = b - a;

            if ( diff.X >= Width >> 1 )
                diff.X -= Width;
            else if ( diff.X < -( Width >> 1 ) )
                diff.X += Width;

            if ( diff.Y >= Height >> 1 )
                diff.Y -= Height;
            else if ( diff.Y < -( Height >> 1 ) )
                diff.Y += Height;

            return diff;
        }

        public Vector2 Wrap( Vector2 pos )
        {
            pos.X -= (int) Math.Floor( pos.X / Width ) * Width;
            pos.Y -= (int) Math.Floor( pos.Y / Height ) * Height;

            return pos;
        }

        public void SplashBlood( Vector2 pos, float force )
        {
            if ( force <= 0.0f )
                return;

            int count = (int) ( force * 4.0f );

            for ( int i = 0; i < count; ++i )
            {
                float dist = i * 0.125f;

                float x = pos.X + ( Tools.Random.NextSingle() * 2.0f - 1.0f ) * dist;
                float y = pos.Y + ( Tools.Random.NextSingle() * 2.0f - 1.0f ) * dist;

                TraceResult res = Trace.Quick( this, pos, new Vector2( x, y ), false, true, new Vector2( 0.25f, 0.25f ) );
                
                myBloodMap.Add(
                    (int) ( res.End.X * 2.0f ),
                    (int) ( res.End.Y * 2.0f ),
                    (byte) Tools.Random.Next( 8, 64 )
                );
            }
        }

        public Block GetBlock( Vector2 pos )
        {
            return RootDistrict.GetBlock( pos.X, pos.Y );
        }

        public Block GetBlock( float x, float y )
        {
            return RootDistrict.GetBlock( x, y );
        }

        public void UpdateGeometryVertexBuffer()
        {
            int count = RootDistrict.GetGeometryVertexCount();
            float[] verts = new float[ count * myGeomVertexBuffer.Stride ];
            int i = 0;
            RootDistrict.GetGeometryVertices( verts, ref i );
            myGeomVertexBuffer.SetData( verts );
        }

        public void UpdatePathVertexBuffer()
        {
            int count = RootDistrict.GetPathVertexCount();
            float[] verts = new float[ count * myPathVertexBuffer.Stride ];
            int i = 0;
            RootDistrict.GetPathVertices( verts, ref i );
            myPathVertexBuffer.SetData( verts );
        }

        public void Think( double dt )
        {
            RootDistrict.Think( dt );
            RootDistrict.PostThink();
        }

        public void RenderGeometry( GeometryShader shader, bool baseOnly = false )
        {
            shader.SetTexture( "bloodmap", myBloodMap );
            myGeomVertexBuffer.StartBatch( shader );
            RootDistrict.RenderGeometry( myGeomVertexBuffer, shader, baseOnly );
            myGeomVertexBuffer.EndBatch( shader );
        }

        public void RenderEntities( FlatEntityShader shader )
        {
            RootDistrict.RenderEntities( shader );
        }

        public void RenderPaths( DebugTraceShader shader )
        {
            if ( !mySetPathVB )
            {
                UpdatePathVertexBuffer();
                mySetPathVB = true;
            }

            myPathVertexBuffer.StartBatch( shader );
            RootDistrict.RenderPaths( myPathVertexBuffer, shader );
            myPathVertexBuffer.EndBatch( shader );
        }

        public IEnumerator<Block> GetEnumerator()
        {
            return new DistrictEnumerator( RootDistrict );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            myGeomVertexBuffer.Dispose();
        }
    }
}
