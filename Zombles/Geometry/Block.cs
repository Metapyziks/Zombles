using System;

using Zombles.Graphics;

namespace Zombles.Geometry
{
    public class Block
    {
        private Tile[,] myTiles;
        private VertexBuffer myVB;

        private float[] myVerts;

        private bool myVertsChanged;

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
            myVerts = new float[ 0 ];

            myVertsChanged = false;
        }

        public void BuildTiles( TileBuilder[,] tiles )
        {
            lock ( myTiles )
            {
                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                        myTiles[ x, y ] = tiles[ x, y ].Create( X + x, Y + y );

                myVertsChanged = true;
            }

            UpdateVertexBuffer();
        }

        private void UpdateVertexBuffer()
        {
            myVertsChanged = false;

            lock ( myTiles )
            {
                int count = 0;
                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                        count += myTiles[ x, y ].GetVertexCount();

                myVerts = new float[ count * 3 ];

                int i = 0;
                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                        myTiles[ x, y ].GetVertices( myVerts, ref i );

            }
            lock( myVB )
                myVB.SetData( myVerts );
        }

        public void Render( GeometryShader shader )
        {
            lock( myVB )
                myVB.Render( shader );

            //shader.Render( myVerts );
        }
    }
}
