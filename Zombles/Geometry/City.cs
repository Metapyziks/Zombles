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

        public District RootDistrict { get; private set; }

        public int Width { get { return RootDistrict.Width; } }
        public int Height { get { return RootDistrict.Height; } }

        public City( int width, int height )
        {
            RootDistrict = new District( 0, 0, width, height );
            myVertexBuffer = new VertexBuffer( 3 );
        }

        public Block GetBlock( Vector2 pos )
        {
            return RootDistrict.GetBlock( pos );
        }

        public void UpdateVertexBuffer()
        {
            int count = RootDistrict.GetVertexCount();
            float[] verts = new float[ count * 3 ];
            int i = 0;
            RootDistrict.GetVertices( verts, ref i );
            myVertexBuffer.SetData( verts );
        }

        public void Render( GeometryShader shader, bool baseOnly = false )
        {
            myVertexBuffer.StartBatch( shader );
            RootDistrict.Render( myVertexBuffer, shader, baseOnly );
            myVertexBuffer.EndBatch( shader );
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
