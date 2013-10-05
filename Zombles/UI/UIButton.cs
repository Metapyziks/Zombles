using System;

using OpenTK;

using Zombles.Graphics;
using OpenTK.Graphics;

namespace Zombles.UI
{
    public class UIButton : UIObject
    {
        private FrameSprite _buttonSprite;
        private UILabel _label;
        private bool _centreText;

        public Color4 Colour;

        public String Text
        {
            get
            {
                return _label.Text;
            }
            set
            {
                _label.Text = value;
                RepositionText();
            }
        }

        public bool CentreText
        {
            get
            {
                return _centreText;
            }
            set
            {
                _centreText = value;
                RepositionText();
            }
        }

        public UIButton(float scale = 1.0f)
            : this(new Vector2(), new Vector2(), scale)
        {

        }

        public UIButton(Vector2 size, float scale = 1.0f)
            : this(size, new Vector2(), scale)
        {

        }

        public UIButton(Vector2 size, Vector2 position, float scale = 1.0f)
            : base(size, position)
        {
            Colour = Color4.White;

            PaddingLeft = PaddingTop = PaddingRight = PaddingBottom = 4.0f * scale;

            _buttonSprite = new FrameSprite(PanelsTexture, scale) {
                SubrectSize = new Vector2(16, 16),
                SubrectOffset = new Vector2(32, 16),
                FrameTopLeftOffet = new Vector2(4, 4),
                FrameBottomRightOffet = new Vector2(4, 4),
                Size = size
            };

            _label = new UILabel(Font.Large, scale);

            AddChild(_label);
        }

        private void RepositionText()
        {
            _label.Top = (InnerHeight - _label.Height) / 2.0f;

            if (CentreText)
                _label.Left = (InnerWidth - _label.Width) / 2.0f;
            else
                _label.Left = 0.0f;
        }

        protected override void OnMouseEnter(Vector2 mousePos)
        {
            if (IsVisible && IsEnabled)
                _buttonSprite.SubrectLeft = 48.0f;
        }

        protected override void OnMouseLeave(Vector2 mousePos)
        {
            if (IsVisible && IsEnabled)
                _buttonSprite.SubrectLeft = 32.0f;
        }

        protected override void OnDisable()
        {
            _buttonSprite.SubrectLeft = 32.0f;
        }

        protected override void OnRender(SpriteShader shader, Vector2 renderPosition = new Vector2())
        {
            _buttonSprite.Position = renderPosition;
            _buttonSprite.Colour = (IsEnabled ? Colour : DisabledColour);
            _buttonSprite.Render(shader);
        }
    }
}
