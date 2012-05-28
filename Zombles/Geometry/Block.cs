using System;
using System.Collections.Generic;

using Zombles.Graphics;
using Zombles.Entities;

namespace Zombles.Geometry
{
    public class Block
    {
        public readonly City City;
        public readonly District District;

        private Tile[,] myTiles;
        private List<Entity> myEnts;

        private int myBaseVertCount;
        private int myTopVertCount;
        private int myVertOffset;

        public readonly int X;
        public readonly int Y;

        public readonly int Width;
        public readonly int Height;

        public Block( District district )
        {
            City = district.City;
            District = district;

            X = district.X;
            Y = district.Y;

            Width = district.Width;
            Height = district.Height;

            myTiles = new Tile[ Width, Height ];
            myEnts = new List<Entity>();
        }

        public void BuildTiles( TileBuilder[,] tiles )
        {
            lock ( myTiles )
                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                        myTiles[ x, y ] = tiles[ x, y ].Create( X + x, Y + y );
        }

        internal void AddEntity( Entity ent )
        {
            myEnts.Add( ent );
        }

        internal void RemoveEntity( Entity ent )
        {
            myEnts.Remove( ent );
        }

        public void Think( double dt )
        {
            for ( int i = myEnts.Count - 1; i >= 0; --i )
                myEnts[ i ].Think( dt );
        }

        public void PostThink()
        {
            for( int i = myEnts.Count - 1; i >= 0; --i )
                myEnts[ i ].UpdateBlock();
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

        public void RenderGeometry( VertexBuffer vb, GeometryShader shader, bool baseOnly = false )
        {
            vb.Render( shader, myVertOffset, ( baseOnly ? myBaseVertCount : myBaseVertCount + myTopVertCount ) );
        }

        public void RenderEntities( FlatEntityShader shader )
        {
            foreach ( Entity ent in myEnts )
                if ( ent.HasComponent<Render>() )
                    ent.GetComponent<Render>().OnRender( shader );
        }
    }
}
