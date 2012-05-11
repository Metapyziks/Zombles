using System;
using System.Collections.Generic;

using ResourceLib;

namespace Zombles.Graphics
{
    public static class TileManager
    {
        internal static Texture2DArray TexArray { get; private set; }

        public static void Initialize()
        {
            KeyValuePair<String, Texture2D>[] textures = Res.GetAll<Texture2D>();

            List<String> tileNames = new List<string>();

            foreach ( KeyValuePair<String, Texture2D> keyVal in textures )
                if ( keyVal.Key.StartsWith( "images_tiles_" ) && keyVal.Value.Width == 8 && keyVal.Value.Height == 8 )
                    tileNames.Add( keyVal.Key );

            TexArray = new Texture2DArray( 8, 8, tileNames.ToArray() );
        }

        public static ushort GetTileIndex( String name )
        {
            if ( name == "" || name == null )
                return 0xffff;

            if ( !name.StartsWith( "images_tiles_" ) )
                name = "images_tiles_" + name;

            return TexArray.GetTextureIndex( name );
        }
    }
}
