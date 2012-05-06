using System;
using System.Collections.Generic;

using Zombles.Graphics;

namespace Zombles.Geometry
{
    public class TileBuilder
    {
        private List<ushort>[] myWallTileIndices;

        public byte WallHeight { get; private set; }
        public byte RoofHeight { get; private set; }

        public ushort FloorTileIndex { get; private set; }
        public ushort RoofTileIndex { get; private set; }

        public TileBuilder()
        {
            WallHeight = 0;
            RoofHeight = 0;

            FloorTileIndex = 0xffff;
            RoofTileIndex = 0xffff;

            myWallTileIndices = new List<ushort>[ 4 ];
            for ( int i = 0; i < 4; ++i )
                myWallTileIndices[ i ] = new List<ushort>();
        }

        public void SetFloor()
        {
            FloorTileIndex = 0xffff;
        }

        public void SetFloor( String textureName )
        {
            FloorTileIndex = TileManager.GetTileIndex( textureName );
        }

        public void SetRoof()
        {
            RoofHeight = 0;
            RoofTileIndex = 0xffff;
        }

        public void SetRoof( String textureName )
        {
            RoofTileIndex = TileManager.GetTileIndex( textureName );
        }

        public void SetRoof( int height, String textureName )
        {
            RoofHeight = (byte) height;
            RoofTileIndex = TileManager.GetTileIndex( textureName );
        }

        public ushort GetWall( Face face, int level )
        {
            return GetWall( face.GetIndex(), level );
        }

        public ushort GetWall( int faceIndex, int level )
        {
            List<ushort> wall = myWallTileIndices[ faceIndex ];

            if ( wall.Count <= level )
                return 0xffff;

            return wall[ level ];
        }

        public void SetWall( Face face, int level )
        {
            SetWallRange( face.GetIndex(), level, 1, 0xffff );
        }

        public void SetWall( int faceIndex, int level )
        {
            SetWallRange( faceIndex, level, 1, 0xffff );
        }

        public void SetWall( Face face, int level, String textureName )
        {
            SetWallRange( face.GetIndex(), level, 1, TileManager.GetTileIndex( textureName ) );
        }

        public void SetWall( Face face, int level, ushort tileIndex )
        {
            SetWallRange( face.GetIndex(), level, 1, tileIndex );
        }

        public void SetWall( int faceIndex, int level, String textureName )
        {
            SetWallRange( faceIndex, level, 1, TileManager.GetTileIndex( textureName ) );
        }

        public void SetWall( int faceIndex, int level, ushort tileIndex )
        {
            SetWallRange( faceIndex, level, 1, tileIndex );
        }

        public void SetWallRange( Face face, int level, int height )
        {
            SetWallRange( face.GetIndex(), level, height, 0xffff );
        }

        public void SetWallRange( int faceIndex, int level, int height )
        {
            SetWallRange( faceIndex, level, height, 0xffff );
        }

        public void SetWallRange( Face face, int level, int height, String textureName )
        {
            SetWallRange( face.GetIndex(), level, height, TileManager.GetTileIndex( textureName ) );
        }

        public void SetWallRange( Face face, int level, int height, ushort tileIndex )
        {
            SetWallRange( face.GetIndex(), level, height, tileIndex );
        }

        public void SetWallRange( int faceIndex, int level, int height, String textureName )
        {
            SetWallRange( faceIndex, level, height, TileManager.GetTileIndex( textureName ) );
        }

        public void SetWallRange( int faceIndex, int level, int height, ushort tileIndex )
        {
            List<ushort> wall = myWallTileIndices[ faceIndex ];

            while ( wall.Count < level + height )
                wall.Add( 0xffff );

            WallHeight = Math.Max( (byte) ( level + height ), WallHeight );

            for( int i = 0; i < height; ++ i )
                wall[ level + i ] = tileIndex;
        }

        public ushort[ , ] GetWallTileIndices()
        {
            ushort[ , ] indices = new ushort[ 4, WallHeight ];
            for ( int i = 0; i < 4; ++i )
            {
                for ( int j = 0; j < WallHeight; ++j )
                {
                    if ( j < myWallTileIndices[ i ].Count )
                        indices[ i, j ] = myWallTileIndices[ i ][ j ];
                    else
                        indices[ i, j ] = 0xffff;
                }
            }
            return indices;
        }

        public Tile Create( int x, int y )
        {
            return new Tile( x, y, this );
        }
    }
}
