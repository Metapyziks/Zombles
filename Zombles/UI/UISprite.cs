using OpenTK;
using OpenTK.Graphics;
using OpenTKTK.Scene;
using OpenTKTK.Shaders;
using Zombles.Graphics;

namespace Zombles.UI
{
    public class UISprite : UIObject
    {
        private Sprite _sprite;

        public Color4 Colour
        {
            get
            {
                return _sprite.Colour;
            }
            set
            {
                _sprite.Colour = value;
            }
        }

        public UISprite(Sprite sprite)
            : this(sprite, new Vector2())
        {

        }

        public UISprite(Sprite sprite, Vector2 position)
            : base(sprite.Size, position)
        {
            _sprite = sprite;
        }

        protected override Vector2 OnSetSize(Vector2 newSize)
        {
            _sprite.Size = newSize;

            return base.OnSetSize(newSize);
        }

        protected override bool CheckPositionWithinBounds(Vector2 pos)
        {
            return false;
        }

        protected override void OnRender(SpriteShader shader, Vector2 renderPosition = new Vector2())
        {
            _sprite.Position = renderPosition;

            _sprite.Render(shader);
        }
    }
}
