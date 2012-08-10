using System;
using System.Collections.Generic;

using Zombles.Graphics;
using Zombles.Entities;

namespace Zombles.Geometry
{
    public class Block : IEnumerable<Entity>
    {
        private Tile[,] myTiles;
        private List<Entity> myEnts;

        private int myBaseGeomVertCount;
        private int myTopGeomVertCount;
        private int myGeomVertOffset;

        private int myPathVertCount;
        private int myPathVertOffset;

        private List<PathEdge> myPaths;

        public readonly City City;
        public readonly District District;

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

        public Tile this[ int x, int y ]
        {
            get
            {
                return myTiles[ x - X, y - Y ];
            }
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

        public int GetGeometryVertexCount()
        {
            myBaseGeomVertCount = 0;
            myTopGeomVertCount = 0;

            for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
            {
                myBaseGeomVertCount += myTiles[ x, y ].GetBaseVertexCount();
                myTopGeomVertCount += myTiles[ x, y ].GetTopVertexCount();
            }

            return myBaseGeomVertCount + myTopGeomVertCount;
        }

        public void GetGeometryVertices( float[] verts, ref int i )
        {
            myGeomVertOffset = i / 3;

            lock ( myTiles )
            {
                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                    myTiles[ x, y ].GetBaseVertices( verts, ref i );

                for ( int x = 0; x < Width; ++x ) for ( int y = 0; y < Height; ++y )
                     myTiles[ x, y ].GetTopVertices( verts, ref i );
            }
        }

        public int GetPathVertexCount()
        {
            myPathVertCount = 0;
            myPaths = new List<PathEdge>();

            foreach ( Entity ent in this )
            {
                if ( ent.HasComponent<Waypoint>() )
                {
                    Waypoint waypoint = ent.GetComponent<Waypoint>();
                    foreach ( PathEdge edge in waypoint.Connections )
                    {
                        if ( edge.EndWaypoint.Entity.ID < waypoint.Entity.ID || !edge.EndWaypoint.IsConnected( waypoint ) )
                        {
                            myPathVertCount += 2;
                            myPaths.Add( edge );
                        }
                    }
                }
            }

            return myPathVertCount;
        }

        public void GetPathVertices( float[] verts, ref int i )
        {
            myPathVertOffset = i / 2;

            foreach ( PathEdge edge in myPaths )
            {
                verts[ i++ ] = edge.Origin.X;
                verts[ i++ ] = edge.Origin.Y;
                verts[ i++ ] = edge.Origin.X + edge.Vector.X;
                verts[ i++ ] = edge.Origin.Y + edge.Vector.Y;
            }
        }

        public void RenderGeometry( VertexBuffer vb, GeometryShader shader, bool baseOnly = false )
        {
            vb.Render( shader, myGeomVertOffset, ( baseOnly ? myBaseGeomVertCount : myBaseGeomVertCount + myTopGeomVertCount ) );
        }

        public void RenderEntities( FlatEntityShader shader )
        {
            foreach ( Entity ent in myEnts )
                if ( ent.HasComponent<Render2D>() )
                    ent.GetComponent<Render2D>().OnRender( shader );
        }

        public void RenderPaths( VertexBuffer vb, DebugTraceShader shader )
        {
            vb.Render( shader, myPathVertOffset, myPathVertCount );
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return myEnts.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return myEnts.GetEnumerator();
        }
    }
}
