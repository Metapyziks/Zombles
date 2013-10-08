using System;
using System.Linq;
using System.Drawing;

using ResourceLibrary;

using OpenTK.Graphics.OpenGL;

using OpenTKTK.Textures;

namespace Zombles.Graphics
{
    public class Texture2DArray : Texture
    {
        private static int GetNextPowerOfTwo(int depth)
        {
            int val = 1;
            while (val < depth)
                val <<= 1;
            return val;
        }

        private ResourceLocator[] _names;
        private BitmapTexture2D[] _textures;

        private UInt32[] _data;

        public Texture2DArray(int width, int height, params ResourceLocator[] textureLocators)
            : base(TextureTarget.Texture2DArray, width, height, GetNextPowerOfTwo(textureLocators.Length))
        {
            _names = textureLocators;
            _textures = new BitmapTexture2D[textureLocators.Length];

            for (int i = 0; i < textureLocators.Length; ++i) {
                _textures[i] = new BitmapTexture2D(Archive.Get<Bitmap>(_names[i]));
            }

            int tileLength = width * height;

            _data = new uint[tileLength * Depth];

            for (int i = 0; i < _textures.Length; ++i) {
                Bitmap tile = _textures[i].Bitmap;

                int xScale = tile.Width / width;
                int yScale = tile.Height / height;

                for (int x = 0; x < width; ++x) {
                    for (int y = 0; y < height; ++y) {
                        int tx = x * xScale;
                        int ty = y * yScale;

                        Color clr = tile.GetPixel(tx, ty);

                        _data[i * tileLength + x + y * width]
                            = (UInt32) (clr.R << 24 | clr.G << 16 | clr.B << 08 | clr.A << 00);
                    }
                }
            }
        }

        public ushort GetTextureIndex(params String[] locator)
        {
            for (int i = 0; i < _names.Length; ++i) {
                if (_names[i].Length == locator.Length
                    && _names[i].Zip(locator, (x, y) => x == y).All(x => x)) {
                    return (ushort) i;
                }
            }

            return 0xffff;
        }

        protected override void Load()
        {
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba, Width, Height, Depth, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888, _data);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp);
            
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

            OpenTKTK.Utils.Tools.ErrorCheck("loadtexture");

            _data = null;
        }
    }
}
