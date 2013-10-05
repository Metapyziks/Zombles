using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class LumTexture2D : Texture
    {
        private readonly int _actualSize;
        private byte[,] _data;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public LumTexture2D( int width, int height )
            : base( TextureTarget.Texture2D )
        {
            Width = width;
            Height = height;

            _actualSize = GetNextPOTS( Width, Height );

            _data = new byte[ Width, Height ];
        }

        public Vector2 GetCoords( Vector2 pos )
        {
            return GetCoords( pos.X, pos.Y );
        }

        public Vector2 GetCoords( float x, float y )
        {
            return new Vector2
            {
                X = x / _actualSize,
                Y = y / _actualSize
            };
        }

        public byte this[ int x, int y ]
        {
            get { return _data[ x, y ]; }
            set
            {
                _data[ x, y ] = value;
                Update();
            }
        }

        public void Add( int x, int y, byte value )
        {
            x -= (int) Math.Floor( (float) x / Width ) * Width;
            y -= (int) Math.Floor( (float) y / Height ) * Height;

            if ( value > 255 - _data[ x, y ] )
                _data[ x, y ] = 255;
            else
                _data[ x, y ] += value;

            Update();
        }

        protected override void Load()
        {
            GL.TexEnv( TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Modulate );

            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Alpha8, _actualSize, _actualSize, 0, OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.UnsignedByte, _data );

            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear );
        }
    }
}
