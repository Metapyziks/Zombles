using OpenTK;

using Zombles.Graphics;

namespace Zombles.UI
{
    public class UIPanel : UIObject
    {
        private UISprite _backSprite;

        public OpenTK.Graphics.Color4 Colour
        {
            get
            {
                return _backSprite.Colour;
            }
            set
            {
                _backSprite.Colour = value;
            }
        }

        public UIPanel()
            : this( new Vector2(), new Vector2() )
        {

        }

        public UIPanel( Vector2 size )
            : this( size, new Vector2() )
        {

        }

        public UIPanel( Vector2 size, Vector2 position )
            : base( size, position )
        {
            _backSprite = new UISprite( new Sprite( size.X, size.Y, OpenTK.Graphics.Color4.White ) );
            AddChild( _backSprite );
        }

        protected override Vector2 OnSetSize( Vector2 newSize )
        {
            _backSprite.SetSize( newSize );

            return base.OnSetSize( newSize );
        }
    }
}
