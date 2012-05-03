using System;
using System.Drawing;

using ResourceLib;

using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class Texture2DArray : Texture
    {
        private String[] myNames;
        private Texture2D[] myTextures;

        private UInt32[] myData;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Count { get; private set; }

        public Texture2DArray( int width, int height, params String[] textureNames )
            : base( TextureTarget.Texture2DArray )
        {
            Width = width;
            Height = height;

            myNames = textureNames;
            myTextures = new Texture2D[ textureNames.Length ];

            for ( int i = 0; i < textureNames.Length; ++i )
                myTextures[ i ] = Res.Get<Texture2D>( myNames[ i ] );

            Count = 1;
            while ( Count < textureNames.Length )
                Count <<= 1;

            int tileLength = width * height;

            myData = new uint[ tileLength * Count ];

            for ( int i = 0; i < myTextures.Length; ++i )
            {
                Bitmap tile = myTextures[ i ].Bitmap;

                int xScale = tile.Width / width;
                int yScale = tile.Height / height;

                for ( int x = 0; x < width; ++x )
                {
                    for ( int y = 0; y < height; ++y )
                    {
                        int tx = x * xScale;
                        int ty = y * yScale;

                        Color clr = tile.GetPixel( tx, ty );

                        myData[ i * tileLength + x + y * width ]
                            = (UInt32) ( clr.R << 24 | clr.G << 16 | clr.B << 08 | clr.A << 00 );
                    }
                }
            }
        }

        public int GetTextureIndex( String texture )
        {
            return Array.IndexOf( myNames, texture );
        }

        protected override void Load()
        {
            GL.TexParameter( TextureTarget.Texture2DArray,
                TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest );
            GL.TexParameter( TextureTarget.Texture2DArray,
                TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest );
            GL.TexParameter( TextureTarget.Texture2DArray,
                TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat );
            GL.TexParameter( TextureTarget.Texture2DArray,
                TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat );
            GL.TexImage3D( TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba,
                Width, Height, Count, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888, myData );
        }
    }
}
