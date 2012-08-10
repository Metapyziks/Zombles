using System;

using OpenTK;

namespace Zombles.Geometry
{
    public class Tile
    {
        public readonly int X;
        public readonly int Y;

        public readonly byte WallHeight;
        public readonly byte FloorHeight;
        public readonly byte RoofHeight;
        public readonly ushort FloorTileIndex;
        public readonly ushort RoofTileIndex;

        public readonly ushort[ , ] WallTileIndices;

        public Tile( int x, int y, TileBuilder builder )
        {
            X = x;
            Y = y;

            WallHeight = builder.WallHeight;
            FloorHeight = builder.FloorHeight;
            RoofHeight = builder.RoofHeight;
            FloorTileIndex = builder.FloorTileIndex;
            RoofTileIndex = builder.RoofTileIndex;
            WallTileIndices = builder.GetWallTileIndices();
        }

        public bool IsWallSolid( Face face )
        {
            if ( FloorHeight > 0 )
                return true;

            if ( WallHeight == 0 )
                return false;

            return WallTileIndices[ face.GetIndex(), 0 ] != 0xffff;
        }

        public int GetBaseVertexCount()
        {
            int count = 0;

            if ( FloorHeight <= 2 && FloorTileIndex != 0xffff )
                ++count;
            if ( RoofHeight > FloorHeight && RoofHeight == 1 && RoofTileIndex != 0xffff )
                ++count;
            int i;
            for ( i = FloorHeight; i < Math.Min( WallHeight, (byte) 2 ); ++i )
                for ( int j = 0; j < 4; ++j )
                    if ( WallTileIndices[ j, i ] != 0xffff )
                        ++count;

            return count * 4;
        }

        public int GetTopVertexCount()
        {
            int count = 0;

            if ( FloorHeight > 2 && FloorTileIndex != 0xffff )
                ++count;
            if ( RoofHeight > FloorHeight && RoofHeight > 1 && RoofTileIndex != 0xffff )
                ++count;
            int i;
            for ( i = Math.Max( FloorHeight, (byte) 2 ); i < WallHeight; ++i )
                for ( int j = 0; j < 4; ++j )
                    if ( WallTileIndices[ j, i ] != 0xffff )
                        ++count;

            return count * 4;
        }

        public void GetBaseVertices( float[] verts, ref int offset )
        {
            if( FloorHeight <= 2 )
                GetFloorVertices( FloorHeight, FloorTileIndex, verts, ref offset );

            if ( RoofHeight > FloorHeight && RoofHeight == 1 )
                GetFloorVertices( RoofHeight, RoofTileIndex, verts, ref offset );

            for ( int i = FloorHeight; i < Math.Min( WallHeight, (byte) 2 ); ++i )
                for ( int j = 0; j < 4; ++j )
                    GetWallVertices( (Face) ( 1 << j ), i, WallTileIndices[ j, i ], verts, ref offset );
        }

        public void GetTopVertices( float[] verts, ref int offset )
        {
            if ( FloorHeight > 2 )
                GetFloorVertices( FloorHeight, FloorTileIndex, verts, ref offset );

            if ( RoofHeight > FloorHeight && RoofHeight > 1 )
                GetFloorVertices( RoofHeight, RoofTileIndex, verts, ref offset );

            for ( int i = Math.Max( FloorHeight, (byte) 2 ); i < WallHeight; ++i )
                for ( int j = 0; j < 4; ++j )
                    GetWallVertices( (Face) ( 1 << j ), i, WallTileIndices[ j, i ], verts, ref offset );
        }

        private void GetFloorVertices( int level, ushort tile, float[] verts, ref int i )
        {
            if ( tile == 0xffff )
                return;

            int tt = ( tile << ( 4 + 4 ) ) | ( ( level & 0xf ) << 4 )
                | ( level == FloorHeight && RoofHeight > FloorHeight && RoofTileIndex != 0xffff ? 8 : 0 );

            verts[ i++ ] = X + 0.0f; verts[ i++ ] = Y + 0.0f; verts[ i++ ] = tt | 0x0;
            verts[ i++ ] = X + 1.0f; verts[ i++ ] = Y + 0.0f; verts[ i++ ] = tt | 0x1;
            verts[ i++ ] = X + 1.0f; verts[ i++ ] = Y + 1.0f; verts[ i++ ] = tt | 0x5;
            verts[ i++ ] = X + 0.0f; verts[ i++ ] = Y + 1.0f; verts[ i++ ] = tt | 0x4;
        }

        private void GetWallVertices( Face face, int level, ushort tile, float[] verts, ref int i )
        {
            if ( tile == 0xffff )
                return;

            Vector3 tl, br;

            switch ( face )
            {
                case Face.West:
                    tl = new Vector3( X + 0.0f, level + 0.5f, Y + 1.0f );
                    br = new Vector3( X + 0.0f, level + 0.0f, Y + 0.0f );
                    break;
                case Face.North:
                    tl = new Vector3( X + 0.0f, level + 0.5f, Y + 0.0f );
                    br = new Vector3( X + 1.0f, level + 0.0f, Y + 0.0f );
                    break;
                case Face.East:
                    tl = new Vector3( X + 1.0f, level + 0.5f, Y + 0.0f );
                    br = new Vector3( X + 1.0f, level + 0.0f, Y + 1.0f );
                    break;
                case Face.South:
                    tl = new Vector3( X + 1.0f, level + 0.5f, Y + 1.0f );
                    br = new Vector3( X + 0.0f, level + 0.0f, Y + 1.0f );
                    break;
                default:
                    return;
            }

            int texData = tile << ( 4 + 4 );
            int shade = ( ( (int) face ) & 0xa ) != 0 ? 8 : 0;
            int tt = texData | ( ( ( level + 1 ) & 0xf ) << 4 ) | shade;
            int bt = texData | ( ( level & 0xf ) << 4 ) | shade;

            int ol = ( level & 0x1 ) << 1;

            int xFace = ( ( (int) face & 0xa ) != 0 ? 0x1000 : 0 );
            int yFace = ( ( (int) face & 0xc ) != 0 ? 0x1000 : 0 );

            verts[ i++ ] = ( (int) tl.X & 0xfff ) | xFace;
            verts[ i++ ] = ( (int) tl.Z & 0xfff ) | yFace;
            verts[ i++ ] = tt | ( 0x0 + ol );
            verts[ i++ ] = ( (int) br.X & 0xfff ) | xFace;
            verts[ i++ ] = ( (int) br.Z & 0xfff ) | yFace;
            verts[ i++ ] = tt | ( 0x1 + ol );
            verts[ i++ ] = ( (int) br.X & 0xfff ) | xFace;
            verts[ i++ ] = ( (int) br.Z & 0xfff ) | yFace;
            verts[ i++ ] = bt | ( 0x3 + ol );
            verts[ i++ ] = ( (int) tl.X & 0xfff ) | xFace;
            verts[ i++ ] = ( (int) tl.Z & 0xfff ) | yFace;
            verts[ i++ ] = bt | ( 0x2 + ol );
        }
    }
}
