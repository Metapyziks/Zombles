using System;
using System.Collections.Generic;

using ResourceLib;

namespace Zombles.Graphics
{
    public class TextureManager
    {
        public static TextureManager Tiles { get; private set; }
        public static TextureManager Ents { get; private set; }

        public static void Initialize()
        {
            Tiles = new TextureManager( "images_tiles_" );
            Ents = new TextureManager( "images_ents_" );
        }

        public readonly String Prefix;

        internal Texture2DArray TexArray { get; private set; }

        private TextureManager( String filePrefix )
        {
            Prefix = filePrefix;

            KeyValuePair<String, Texture2D>[] textures = Res.GetAll<Texture2D>();

            List<String> tileNames = new List<string>();

            foreach ( KeyValuePair<String, Texture2D> keyVal in textures )
                if ( keyVal.Key.StartsWith( Prefix ) && keyVal.Value.Width == 8 && keyVal.Value.Height == 8 )
                    tileNames.Add( keyVal.Key );

            tileNames.Sort();

            TexArray = new Texture2DArray( 8, 8, tileNames.ToArray() );
        }

        public ushort GetIndex( String name )
        {
            if ( name == "" || name == null )
                return 0xffff;

            if ( !name.StartsWith( Prefix ) )
                name = Prefix + name;

            return TexArray.GetTextureIndex( name );
        }
    }
}
