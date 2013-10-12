using System;

using OpenTK;

namespace Zombles.UI
{
    public class UIMessageBox : UIWindow
    {
        private UILabel _text;
        private bool _centreText;

        public bool CentreText
        {
            get
            {
                return _centreText;
            }
            set
            {
                _centreText = value;
                if ( _centreText )
                    _text.Centre();
                else
                    _text.Position = new Vector2( 4, 4 );
            }
        }

        public String Text
        {
            get
            {
                return _text.Text;
            }
            set
            {
                _text.Text = value;
                if( CentreText )
                    _text.Centre();
            }
        }

        public UIMessageBox( String message, String title, bool closeButton = true )
            : base( new Vector2( 480, 64 ) )
        {
            CanClose = closeButton;
            _centreText = false;

            Title = title;

            _text = new UILabel( Zombles.Graphics.PixelFont.Large )
            {
                Text = message,
                Position = new Vector2( 4, 4 )
            };
            AddChild( _text );
        }
    }
}
