using System;

using Zombles.Graphics;

namespace Zombles.Geometry
{
    public class Block
    {
        private Tile[,] myTiles;
        private VertexBuffer myVB;
        private int myBaseVertCount;

        public readonly int X;
        public readonly int Y;

        public readonly int Width;
        public readonly int Height;

        public Block( int x, int y, int width, int height )
        {
            X = x;
            Y = y;

            Width = width;
            Height = height;

            myTiles = new Tile[ width, height ];
            myVB = new VertexBuffer( 3 );
        }

        public void BuildTiles( TileBuilder[,] tiles )
        {
            lock ( myTiles )
                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                        myTiles[ x, y ] = tiles[ x, y ].Create( X + x, Y + y );

            UpdateVertexBuffer();
        }

        private void UpdateVertexBuffer()
        {
            float[] verts;

            lock ( myTiles )
            {
                int baseCount = 0;
                int topCount = 0;
                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                {
                    baseCount += myTiles[ x, y ].GetBaseVertexCount();
                    topCount += myTiles[ x, y ].GetTopVertexCount();
                }

                verts = new float[ ( baseCount + topCount ) * 3 ];
                myBaseVertCount = baseCount;

                int i = 0;
                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                    myTiles[ x, y ].GetBaseVertices( verts, ref i );

                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                     myTiles[ x, y ].GetTopVertices( verts, ref i );

            }
            lock( myVB )
                myVB.SetData( verts );
        }

        public void Render( GeometryShader shader, bool baseOnly = false )
        {
            lock( myVB )
                myVB.Render( shader, 0, ( baseOnly ? myBaseVertCount : -1 ) );
        }
    }
}
