using System;

using Zombles.Graphics;

namespace Zombles.Geometry
{
    public class TileBuilder
    {
        public byte Height { get; private set; }

        public ushort FloorTileIndex { get; private set; }
        public ushort RoofTileIndex { get; private set; }

        public ushort[ , ] WallTileIndices { get; private set; }

        public TileBuilder( int height )
        {
            Height = (byte) Math.Min( height, 8 );

            FloorTileIndex = 0xffff;
            RoofTileIndex = 0xffff;

            WallTileIndices = new ushort[ 4, Height ];

            for ( int i = 0; i < height; ++i )
                for ( int j = 0; j < 4; ++j )
                    WallTileIndices[ j, i ] = 0xffff;
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
            RoofTileIndex = 0xffff;
        }

        public void SetRoof( String textureName )
        {
            RoofTileIndex = TileManager.GetTileIndex( textureName );
        }

        public void SetWall( Face face )
        {
            int faceIndex = face.GetIndex();
            for ( int i = 0; i < Height; ++i )
                WallTileIndices[ faceIndex, i ] = 0xffff;
        }

        public void SetWall( Face face, String textureName )
        {
            ushort tileIndex = TileManager.GetTileIndex( textureName );
            int faceIndex = face.GetIndex();
            for ( int i = 0; i < Height; ++i )
                WallTileIndices[ faceIndex, i ] = tileIndex;
        }

        public void SetWall( Face face, int level )
        {
            int faceIndex = face.GetIndex();
            WallTileIndices[ faceIndex, level ] = 0xffff;
        }

        public void SetWall( Face face, int level, String textureName )
        {
            ushort tileIndex = TileManager.GetTileIndex( textureName );
            int faceIndex = face.GetIndex();
            WallTileIndices[ faceIndex, level ] = tileIndex;
        }

        public void CullHiddenWalls( TileBuilder neighbour, Face face )
        {
            int height = Math.Min( Height, neighbour.Height );
            int mfi = face.GetIndex();
            int nfi = ( mfi + 2 ) & 0x3;
            for ( int i = 0; i < height; ++i )
                if ( WallTileIndices[ mfi, i ] != 0xffff && neighbour.WallTileIndices[ nfi, i ] != 0xffff )
                    WallTileIndices[ mfi, i ] = neighbour.WallTileIndices[ nfi, i ] = 0xffff;
        }

        public Tile Create( int x, int y )
        {
            return new Tile( x, y, this );
        }
    }
}
