using ResourceLibrary;

using OpenTK;

using Zombles.Graphics;
using System.Drawing;
using OpenTKTK.Shaders;
using OpenTKTK.Scene;

namespace Zombles.UI
{
    public class UIWindowCloseButton : UIObject
    {
        private Sprite _sprite;

        public UIWindowCloseButton(float scale = 1.0f)
            : this(new Vector2(), scale)
        {

        }

        public UIWindowCloseButton(Vector2 position, float scale = 1.0f)
            : base(new Vector2(), position)
        {
            _sprite = new Sprite(PanelsTexture, scale) {
                SubrectOffset = new Vector2(32, 0),
                SubrectSize = new Vector2(16, 16)
            };

            SetSize(16.0f * scale, 16.0f * scale);

            CanResize = false;
        }

        protected override void OnMouseEnter(Vector2 mousePos)
        {
            _sprite.SubrectLeft = 48.0f;
        }

        protected override void OnMouseLeave(Vector2 mousePos)
        {
            _sprite.SubrectLeft = 32.0f;
        }

        protected override void OnRender(SpriteShader shader, Vector2 renderPosition = new Vector2())
        {
            _sprite.Position = renderPosition;

            _sprite.Render(shader);
        }
    }
}
