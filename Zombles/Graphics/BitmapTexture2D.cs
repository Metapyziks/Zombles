using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using ResourceLib;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class RTexture2DManager : RManager
    {
        public RTexture2DManager()
            : base( typeof( BitmapTexture2D ), 2, "png" )
        {

        }

        public override ResourceItem[] LoadFromFile( String keyPrefix, String fileName, String fileExtension, FileStream stream )
        {
            return new ResourceItem[] { new ResourceItem( keyPrefix + fileName, new BitmapTexture2D( new Bitmap( stream ) ) ) };
        }

        public override Object LoadFromArchive( BinaryReader stream )
        {
            ushort wid = stream.ReadUInt16();
            ushort hei = stream.ReadUInt16();

            Bitmap bmp = new Bitmap( wid, hei );

            for ( int x = 0; x < wid; ++x )
                for ( int y = 0; y < hei; ++y )
                {
                    bmp.SetPixel( x, y, Color.FromArgb(
                        stream.ReadByte(),
                        stream.ReadByte(),
                        stream.ReadByte(),
                        stream.ReadByte()
                    ) );
                }

            return new BitmapTexture2D( bmp );
        }

        public override void SaveToArchive( BinaryWriter stream, Object item )
        {
            BitmapTexture2D tex = (BitmapTexture2D) item;
            Bitmap bmp = tex.Bitmap;

            ushort wid = (ushort) tex.Width;
            ushort hei = (ushort) tex.Height;

            stream.Write( wid );
            stream.Write( hei );

            for ( int x = 0; x < wid; ++x )
                for ( int y = 0; y < hei; ++y )
                {
                    Color pix = bmp.GetPixel( x, y );
                    stream.Write( pix.A );
                    stream.Write( pix.R );
                    stream.Write( pix.G );
                    stream.Write( pix.B );
                }
        }
    }

    public class BitmapTexture2D : Texture
    {
        public static readonly BitmapTexture2D Blank;

        static BitmapTexture2D()
        {
            Bitmap blankBmp = new Bitmap( 1, 1 );
            blankBmp.SetPixel( 0, 0, Color.White );
            Blank = new BitmapTexture2D( blankBmp );
        }

        private readonly int myActualSize;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Bitmap Bitmap { get; private set; }

        public BitmapTexture2D( Bitmap bitmap )
            : base( TextureTarget.Texture2D )
        {
            Width = bitmap.Width;
            Height = bitmap.Height;

            myActualSize = GetNextPOTS( bitmap.Width, bitmap.Height );

            if ( myActualSize == bitmap.Width && myActualSize == bitmap.Height )
                Bitmap = bitmap;
            else
            {
                Bitmap = new Bitmap( myActualSize, myActualSize );

                for ( int x = 0; x < Width; ++x )
                    for ( int y = 0; y < Height; ++y )
                        Bitmap.SetPixel( x, y, bitmap.GetPixel( x, y ) );
            }
        }

        public Vector2 GetCoords( Vector2 pos )
        {
            return GetCoords( pos.X, pos.Y );
        }

        public Vector2 GetCoords( float x, float y )
        {
            return new Vector2
            {
                X = x / myActualSize,
                Y = y / myActualSize
            };
        }

        protected override void Load()
        {
            GL.TexEnv( TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Modulate );

            BitmapData data = Bitmap.LockBits( new Rectangle( 0, 0, Bitmap.Width, Bitmap.Height ), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Bitmap.Width, Bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0 );

            Bitmap.UnlockBits( data );

            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest );
        }
    }
}
