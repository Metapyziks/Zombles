using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zombles.Graphics;

namespace Zombles.Geometry
{
    public class City : IEnumerable<Block>, IDisposable
    {
        private VertexBuffer myVertexBuffer;

        public District RootDistrict { get; private set; }

        public City( int width, int height )
        {
            RootDistrict = new District( 0, 0, width, height );
            myVertexBuffer = new VertexBuffer( 3 );
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
            RootDistrict.Render( myVertexBuffer, shader, baseOnly );
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
