using System;
using System.Drawing;

using OpenTK;

using OpenTKTK.Textures;

using ResourceLibrary;

using Zombles;

namespace Zombles.Graphics
{
    public class Font
    {
        private static Font _sFontDefault;
        private static Font _sFontLarge;

        public static Font Default
        {
            get
            {
                if (_sFontDefault == null)
                    _sFontDefault = new Font("images", "gui", "fontdefault");
                return _sFontDefault;
            }
        }
        public static Font Large
        {
            get
            {
                if (_sFontLarge == null)
                    _sFontLarge = new Font("images", "gui", "fontlarge");
                return _sFontLarge;
            }
        }

        internal readonly BitmapTexture2D Texture;

        public readonly Vector2 CharSize;

        public int CharWidth
        {
            get
            {
                return (int) CharSize.X;
            }
        }

        public int CharHeight
        {
            get
            {
                return (int) CharSize.Y;
            }
        }

        public Font(params String[] charMapLocator)
        {
            Texture = new BitmapTexture2D(Archive.Get<Bitmap>(charMapLocator));

            CharSize = new Vector2(Texture.Width / 16, Texture.Height / 16);
        }

        public Vector2 GetCharOffset(Char character)
        {
            int id = (int) character;

            return new Vector2((id % 16) * CharWidth, (id / 16) * CharHeight);
        }
    }

    public class Text : Sprite
    {
        private String _text;
        private Font _font;
        private float _wrapWidth;

        public Font Font
        {
            get
            {
                return _font;
            }
        }

        public String String
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                VertsChanged = true;
            }
        }

        public float WrapWidth
        {
            get
            {
                return _wrapWidth;
            }
            set
            {
                _wrapWidth = value;
                VertsChanged = true;
            }
        }

        public Text()
            : this(Font.Default)
        {

        }

        public Text(Font font, float scale = 1.0f)
            : base(font.Texture, scale)
        {
            _text = "";
            _font = font;
        }

        protected override float[] FindVerts()
        {
            String text = _text.ApplyWordWrap(Font.CharWidth * Scale.X, WrapWidth);

            int characters = text.Length;

            float[,] mat = new float[,]
            {
                { (float) Math.Cos( Rotation ) * Scale.X, -(float) Math.Sin( Rotation ) * Scale.Y },
                { (float) Math.Sin( Rotation ) * Scale.X,  (float) Math.Cos( Rotation ) * Scale.Y }
            };

            int quads = 0;

            for (int i = 0; i < characters; ++i)
                if (!char.IsWhiteSpace(text[i]))
                    ++quads;

            float[] verts = new float[quads * 8 * 4];

            for (int i = 0, index = 0, x = 0, y = 0; i < characters; ++i)
                GetCharVerts(text[i], verts, ref index, mat, ref x, ref y);

            return verts;
        }

        private void GetCharVerts(char character, float[] verts, ref int index, float[,] rotationMat, ref int x, ref int y)
        {
            if (char.IsWhiteSpace(character)) {
                if (character == '\t')
                    x += 4;
                else if (character == '\n') {
                    y += 1;
                    x = 0;
                } else
                    x += 1;

                return;
            }

            Vector2 subMin = _font.GetCharOffset(character);

            Vector2 tMin = Texture.GetCoords(subMin.X, subMin.Y);
            Vector2 tMax = Texture.GetCoords(subMin.X + _font.CharWidth, subMin.Y + _font.CharHeight);
            float xMin = tMin.X;
            float yMin = tMin.Y;
            float xMax = tMax.X;
            float yMax = tMax.Y;

            float minX = x * _font.CharWidth;
            float minY = y * _font.CharHeight;

            float[,] pos = new float[,]
            {
                { minX, minY },
                { minX + _font.CharWidth, minY },
                { minX + _font.CharWidth, minY + _font.CharHeight },
                { minX, minY + _font.CharHeight }
            };

            for (int i = 0; i < 4; ++i) {
                float xp = pos[i, 0];
                float yp = pos[i, 1];
                pos[i, 0] = X + rotationMat[0, 0] * xp + rotationMat[0, 1] * yp;
                pos[i, 1] = Y + rotationMat[1, 0] * xp + rotationMat[1, 1] * yp;
            }

            Array.Copy(new float[]
            {
                pos[ 0, 0 ], pos[ 0, 1 ], xMin, yMin, Colour.R, Colour.G, Colour.B, Colour.A,
                pos[ 1, 0 ], pos[ 1, 1 ], xMax, yMin, Colour.R, Colour.G, Colour.B, Colour.A,
                pos[ 2, 0 ], pos[ 2, 1 ], xMax, yMax, Colour.R, Colour.G, Colour.B, Colour.A,
                pos[ 3, 0 ], pos[ 3, 1 ], xMin, yMax, Colour.R, Colour.G, Colour.B, Colour.A,
            }, 0, verts, index, 8 * 4);

            index += 8 * 4;
            x += 1;
        }
    }
}
