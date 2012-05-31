using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Graphics;

namespace Zombles.Geometry
{
    public class City : IEnumerable<Block>, IDisposable
    {
        private VertexBuffer myVertexBuffer;
        private LumTexture2D myBloodMap;

        public District RootDistrict { get; private set; }

        public int Width { get { return RootDistrict.Width; } }
        public int Height { get { return RootDistrict.Height; } }

        public int Depth { get { return RootDistrict.Depth; } }

        public City( int width, int height )
        {
            RootDistrict = new District( this, 0, 0, width, height );
            myVertexBuffer = new VertexBuffer( 3 );
            myBloodMap = new LumTexture2D( width * 2, height * 2 );
        }

        public void SplashBlood( Vector2 pos, float force )
        {
            if ( force <= 0.0f )
                return;

            int count = (int) ( force * 4.0f );

            for ( int i = 0; i < count; ++i )
            {
                float dist = i * 0.125f;

                myBloodMap.Add(
                    (int) ( ( pos.X + ( Tools.Random.NextSingle() * 2.0f - 1.0f ) * dist ) * 2.0f ),
                    (int) ( ( pos.Y + ( Tools.Random.NextSingle() * 2.0f - 1.0f ) * dist ) * 2.0f ),
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

        public void UpdateVertexBuffer()
        {
            int count = RootDistrict.GetVertexCount();
            float[] verts = new float[ count * 3 ];
            int i = 0;
            RootDistrict.GetVertices( verts, ref i );
            myVertexBuffer.SetData( verts );
        }

        public void Think( double dt )
        {
            RootDistrict.Think( dt );
            RootDistrict.PostThink();
        }

        public void RenderGeometry( GeometryShader shader, bool baseOnly = false )
        {
            myVertexBuffer.StartBatch( shader );
            RootDistrict.RenderGeometry( myVertexBuffer, shader, baseOnly );
            myVertexBuffer.EndBatch( shader );
        }

        public void RenderBlood( BloodShader shader )
        {
            shader.SetTexture( "bloodmap", myBloodMap );
            RootDistrict.RenderBlood( shader );
        }

        public void RenderEntities( FlatEntityShader shader )
        {
            RootDistrict.RenderEntities( shader );
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
            myVertexBuffer.Dispose();
        }
    }
}
