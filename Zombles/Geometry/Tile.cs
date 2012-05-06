using OpenTK;

namespace Zombles.Geometry
{
    public class Tile
    {
        public readonly int X;
        public readonly int Y;

        public readonly byte WallHeight;
        public readonly byte RoofHeight;
        public readonly ushort FloorTileIndex;
        public readonly ushort RoofTileIndex;

        public readonly ushort[ , ] WallTileIndices;

        public Tile( int x, int y, TileBuilder builder )
        {
            X = x;
            Y = y;

            WallHeight = builder.WallHeight;
            RoofHeight = builder.RoofHeight;
            FloorTileIndex = builder.FloorTileIndex;
            RoofTileIndex = builder.RoofTileIndex;
            WallTileIndices = builder.GetWallTileIndices();
        }

        public bool IsWallSolid( Face face )
        {
            if ( WallHeight == 0 )
                return false;

            return WallTileIndices[ face.GetIndex(), 0 ] != 0xffff;
        }

        public int GetVertexCount()
        {
            int count = 0;

            if ( FloorTileIndex != 0xffff )
                ++count;
            if ( RoofHeight > 0 && RoofTileIndex != 0xffff )
                ++count;
            int i;
            for ( i = 0; i < WallHeight; ++i )
                for ( int j = 0; j < 4; ++j )
                    if ( WallTileIndices[ j, i ] != 0xffff )
                        ++count;

            return count * 4;
        }

        public void GetVertices( float[] verts, ref int offset )
        {
            GetFloorVertices( 0, FloorTileIndex, verts, ref offset );

            if ( RoofHeight > 0 )
                GetFloorVertices( RoofHeight, RoofTileIndex, verts, ref offset );

            for ( int i = 0; i < WallHeight; ++i )
                for ( int j = 0; j < 4; ++j )
                    GetWallVertices( (Face) ( 1 << j ), i, WallTileIndices[ j, i ], verts, ref offset );
        }

        private void GetFloorVertices( int level, ushort tile, float[] verts, ref int i )
        {
            if ( tile == 0xffff )
                return;

            int tt = ( tile << ( 4 + 4 ) ) | ( ( level & 0xf ) << 4 );

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

            verts[ i++ ] = tl.X; verts[ i++ ] = tl.Z; verts[ i++ ] = tt | 0x0;
            verts[ i++ ] = br.X; verts[ i++ ] = br.Z; verts[ i++ ] = tt | 0x1;
            verts[ i++ ] = br.X; verts[ i++ ] = br.Z; verts[ i++ ] = bt | 0x3;
            verts[ i++ ] = tl.X; verts[ i++ ] = tl.Z; verts[ i++ ] = bt | 0x2;
        }
    }
}
