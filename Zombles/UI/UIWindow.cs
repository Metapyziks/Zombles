using System;

using OpenTK;

using ResourceLibrary;

using Zombles.Graphics;

namespace Zombles.UI
{
    public class UIWindow : UIObject
    {
        private float _scale;
        private FrameSprite _frameSprite;
        private UILabel _titleText;
        private UIWindowCloseButton _closeButton;
        private bool _dragging;
        private Vector2 _dragPos;
        private bool _canClose;

        public bool CanDrag;

        public String Title
        {
            get
            {
                return _titleText.Text;
            }
            set
            {
                _titleText.Text = value;
            }
        }

        public bool CanClose
        {
            get
            {
                return _canClose;
            }
            set
            {
                _canClose = value;
                _closeButton.IsEnabled = value;
                _closeButton.IsVisible = value;
            }
        }

        public UIWindow(float scale = 1.0f)
            : this(new Vector2(), new Vector2(), scale)
        {

        }

        public UIWindow(Vector2 size, float scale = 1.0f)
            : this(size, new Vector2(), scale)
        {

        }

        public UIWindow(Vector2 size, Vector2 position, float scale = 1.0f)
            : base(size, position)
        {
            _scale = scale;

            PaddingLeft = 4.0f * scale;
            PaddingTop = 20.0f * scale;
            PaddingRight = 4.0f * scale;
            PaddingBottom = 4.0f * scale;

            _frameSprite = new FrameSprite(PanelsTexture, scale) {
                SubrectSize = new Vector2(32, 32),
                SubrectOffset = new Vector2(0, 0),
                FrameTopLeftOffet = new Vector2(4, 20),
                FrameBottomRightOffet = new Vector2(4, 4),
                Size = size
            };

            _titleText = new UILabel(Font.Large, scale) {
                Position = new Vector2(6 * scale - PaddingLeft, 4 * scale - PaddingTop),
                IsEnabled = false
            };

            AddChild(_titleText);

            _closeButton = new UIWindowCloseButton(new Vector2(size.X - 18.0f * scale - PaddingLeft, 2.0f * scale - PaddingTop), scale);

            _closeButton.Click += delegate(object sender, OpenTK.Input.MouseButtonEventArgs e) {
                Close();
            };

            AddChild(_closeButton);

            CanBringToFront = true;
            CanClose = true;
            CanDrag = true;
        }

        public void Close()
        {
            IsEnabled = false;
            IsVisible = false;

            OnClose();
            if (Closed != null)
                Closed(this, new EventArgs());
        }

        public event EventHandler Closed;

        protected virtual void OnClose()
        {

        }

        protected override Vector2 OnSetSize(Vector2 newSize)
        {
            _frameSprite.Size = newSize;
            _closeButton.Left = newSize.X - 18.0f * _scale - PaddingLeft;

            return base.OnSetSize(newSize);
        }

        protected override void OnMouseDown(Vector2 mousePos, OpenTK.Input.MouseButton mouseButton)
        {
            if (CanDrag && mousePos.Y < 20 * _scale) {
                _dragging = true;
                _dragPos = mousePos;
            }
        }

        protected override void OnMouseUp(Vector2 mousePos, OpenTK.Input.MouseButton mouseButton)
        {
            _dragging = false;
        }

        protected override void OnMouseMove(Vector2 mousePos)
        {
            if (_dragging) {
                if (!CanDrag) {
                    _dragging = false;
                    return;
                }

                Position += mousePos - _dragPos;

                if (Left < 0)
                    Left = 0;
                if (Top < 0)
                    Top = 0;
                if (Right > Parent.InnerWidth)
                    Right = Parent.InnerWidth;
                if (Bottom > Parent.InnerHeight)
                    Bottom = Parent.InnerHeight;
            }
        }

        protected override void OnRender(SpriteShader shader, Vector2 renderPosition = new Vector2())
        {
            _frameSprite.Position = renderPosition;
            _frameSprite.Colour = (IsEnabled ? OpenTK.Graphics.Color4.White : DisabledColour);
            _frameSprite.Render(shader);
        }
    }
}
