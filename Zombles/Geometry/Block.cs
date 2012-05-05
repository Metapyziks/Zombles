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

        public void Generate( int seed = 0 )
        {
            Random rand = ( seed == 0 ? new Random() : new Random( seed ) );

            TileBuilder[,] builders = new TileBuilder[ Width, Height ];

            for ( int x = 0; x < Width; ++x )
            {
                for ( int y = 0; y < Height; ++y )
                {
                    builders[ x, y ] = new TileBuilder( 1 );
                    builders[ x, y ].SetFloor( "floor_concrete_0" );
                }
            }

            for ( int i = 0; i < 64; ++i )
            {
                int x = rand.Next( 1, Width - 1 );
                int y = rand.Next( 1, Height - 1 );

                builders[ x, y ].SetFloor();
                builders[ x, y ].SetRoof( "floor_crate_0" );
                builders[ x + 1, y ].SetWall( Face.West, "wall_crate_0" );
                builders[ x, y + 1 ].SetWall( Face.North, "wall_crate_0" );
                builders[ x - 1, y ].SetWall( Face.East, "wall_crate_0" );
                builders[ x, y - 1 ].SetWall( Face.South, "wall_crate_0" );
            }

            for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
            {
                TileBuilder b = builders[ x, y ];
                if ( x < Width - 1 )  b.CullHiddenWalls( builders[ x + 1, y ], Face.East );
                if ( y < Height - 1 ) b.CullHiddenWalls( builders[ x, y + 1 ], Face.South );
            }

            lock ( myTiles )
            {
                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                    myTiles[ x, y ] = builders[ x, y ].Create( X + x, Y + y );

                myVertsChanged = true;
            }
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
            if ( myVertsChanged )
                UpdateVertexBuffer();

            shader.Render( myVerts );
        }
    }
}
