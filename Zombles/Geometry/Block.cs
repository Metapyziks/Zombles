using System;

using Zombles.Graphics;

namespace Zombles.Geometry
{
    public class Block
    {
        private Tile[,] myTiles;

        private int myBaseVertCount;
        private int myTopVertCount;
        private int myVertOffset;

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
        }

        public void BuildTiles( TileBuilder[,] tiles )
        {
            lock ( myTiles )
                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                        myTiles[ x, y ] = tiles[ x, y ].Create( X + x, Y + y );
        }

        public int GetVertexCount()
        {
            myBaseVertCount = 0;
            myTopVertCount = 0;

            for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
            {
                myBaseVertCount += myTiles[ x, y ].GetBaseVertexCount();
                myTopVertCount += myTiles[ x, y ].GetTopVertexCount();
            }

            return myBaseVertCount + myTopVertCount;
        }

        public void GetVertices( float[] verts, ref int i )
        {
            myVertOffset = i / 3;

            lock ( myTiles )
            {
                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                    myTiles[ x, y ].GetBaseVertices( verts, ref i );

                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                     myTiles[ x, y ].GetTopVertices( verts, ref i );
            }
        }

        public void Render( VertexBuffer vb, GeometryShader shader, bool baseOnly = false )
        {
            vb.Render( shader, myVertOffset, ( baseOnly ? myBaseVertCount : myBaseVertCount + myTopVertCount ) );
        }
    }
}
