using System;
using OpenTK;
using OpenTK.Input;
using OpenTKTK.Scene;
using OpenTKTK.Shaders;
using ResourceLibrary;
using Zombles.Graphics;

namespace Zombles.UI
{
    public class UITextBox : UIObject
    {
        private PixelFont _font;
        private FrameSprite _sprite;
        private UILabel _text;
        private Sprite _underlineChar;

        private DateTime _lastFlashTime;

        public int CharacterLimit;

        public String Text
        {
            get
            {
                return _text.Text;
            }
            set
            {
                if (value.Length <= CharacterLimit)
                    _text.Text = value;
                else
                    _text.Text = value.Substring(0, CharacterLimit);
            }
        }

        public UITextBox(float scale = 1.0f)
            : this(new Vector2(), new Vector2(), scale)
        {

        }

        public UITextBox(Vector2 size, float scale = 1.0f)
            : this(size, new Vector2(), scale)
        {

        }

        public UITextBox(Vector2 size, Vector2 position, float scale = 1.0f)
            : base(size, position)
        {
            PaddingLeft = PaddingTop = PaddingRight = PaddingBottom = 4.0f * scale;

            _sprite = new FrameSprite(PanelsTexture, scale) {
                SubrectSize = new Vector2(16, 16),
                SubrectOffset = new Vector2(0, 32),
                FrameTopLeftOffet = new Vector2(4, 4),
                FrameBottomRightOffet = new Vector2(4, 4),
                Size = size
            };

            _font = PixelFont.Large;
            _text = new UILabel(_font, scale);
            AddChild(_text);

            CharacterLimit = (int) (InnerWidth / (_font.CharWidth * scale));

            _underlineChar = new Sprite(scale * _font.CharWidth, scale * 2.0f, OpenTK.Graphics.Color4.Black);
        }

        protected override void OnFocus()
        {
            _lastFlashTime = DateTime.Now;
        }

        private char KeyToChar(KeyboardKeyEventArgs e)
        {
            var str = e.Key.ToString();

            if (str.Length == 1) {
                return e.Shift ? str[0] : str.ToLower()[0];
            } else if (str.StartsWith("Number") || str.StartsWith("Keypad") && str.Length == 7) {
                return str[6];
            }

            switch (e.Key) {
                case Key.BackSlash:
                    return '\\';
                case Key.BracketLeft:
                    return '(';
                case Key.BracketRight:
                    return ')';
                case Key.Comma:
                    return ',';
                case Key.Space:
                    return ' ';
            }

            return '\0';
        }

        protected virtual bool OnValidateString(ref String str)
        {
            return true;
        }

        protected override void OnKeyPress(KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.BackSpace) {
                if (Text.Length > 0)
                    Text = Text.Substring(0, Text.Length - 1);
            } else if (e.Key == Key.Escape || e.Key == Key.Enter) {
                UnFocus();
            } else {
                char c = KeyToChar(e);

                var str = Text + c;
                if (c != '\0' && OnValidateString(ref str)) {
                    Text = str;
                }
            }
        }

        protected override void OnRender(SpriteShader shader, Vector2 renderPosition = new Vector2())
        {
            _sprite.Position = renderPosition;
            _sprite.Colour = (IsEnabled ? OpenTK.Graphics.Color4.White : DisabledColour);
            _sprite.Render(shader);

            double timeSinceFlash = (DateTime.Now - _lastFlashTime).TotalSeconds;

            if (timeSinceFlash < 0.5 && IsFocused && IsEnabled && Text.Length < CharacterLimit) {
                _underlineChar.Position = renderPosition + new Vector2(PaddingLeft + _text.Width, PaddingTop + Math.Max(_text.Height, _font.CharHeight) - _underlineChar.Height);
                _underlineChar.Render(shader);
            } else if (timeSinceFlash >= 1.0)
                _lastFlashTime = DateTime.Now;
        }
    }
}
