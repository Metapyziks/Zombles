using System;
using System.Linq;
using System.Drawing;

using ResourceLibrary;

using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class Texture2DArray : Texture
    {
        private ResourceLocator[] _names;
        private BitmapTexture2D[] _textures;

        private UInt32[] _data;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Count { get; private set; }

        public Texture2DArray(int width, int height, params ResourceLocator[] textureLocators)
            : base(TextureTarget.Texture2DArray)
        {
            Width = width;
            Height = height;

            _names = textureLocators;
            _textures = new BitmapTexture2D[textureLocators.Length];

            for (int i = 0; i < textureLocators.Length; ++i) {
                _textures[i] = new BitmapTexture2D(Archive.Get<Bitmap>(_names[i]));
            }

            Count = 1;
            while (Count < textureLocators.Length)
                Count <<= 1;

            int tileLength = width * height;

            _data = new uint[tileLength * Count];

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
            GL.TexParameter(TextureTarget.Texture2DArray,
                TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2DArray,
                TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray,
                TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2DArray,
                TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp);
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba,
                Width, Height, Count, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888, _data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

            _data = null;
        }
    }
}
