using System;

using OpenTK;
using OpenTK.Graphics;

using Zombles;
using Zombles.Graphics;

namespace Zombles.UI
{
    public class UILabel : UIObject
    {
        private Font _font;
        private Text _text;

        public String Text
        {
            get
            {
                return _text.String;
            }
            set
            {
                _text.String = value;
                FindSize();
            }
        }

        public Font Font
        {
            get
            {
                return _font;
            }
        }

        public Color4 Colour
        {
            get
            {
                return _text.Colour;
            }
            set
            {
                _text.Colour = value;
            }
        }

        public float WrapWidth
        {
            get
            {
                return _text.WrapWidth;
            }
            set
            {
                _text.WrapWidth = value;
                FindSize();
            }
        }

        public UILabel( Font font, float scale = 1.0f )
            : this( font, new Vector2(), scale )
        {
            
        }

        public UILabel( Font font, Vector2 position, float scale = 1.0f )
            : base( new Vector2(), position )
        {
            _font = font;
            _text = new Text( font, scale );
            Colour = Color4.Black;
            CanResize = false;
            IsEnabled = false;
        }

        protected override void OnRender( SpriteShader shader, Vector2 renderPosition = new Vector2() )
        {
            _text.Position = renderPosition;

            _text.Render( shader );
        }

        private void FindSize()
        {
            String[] lines = _text.String.ApplyWordWrap( Font.CharWidth * _text.Scale.X, _text.WrapWidth ).Split( '\n' );

            int maxLength = 0;

            foreach ( String line in lines )
                if ( line.Length > maxLength )
                    maxLength = line.Length;

            float width = _font.CharWidth * _text.Scale.X * maxLength;
            float height = _font.CharHeight * _text.Scale.Y * lines.Length;

            CanResize = true;
            SetSize( width, height );
            CanResize = false;
        }
    }
}
