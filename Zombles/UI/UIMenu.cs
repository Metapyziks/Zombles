using System;
using System.Collections.Generic;

using OpenTK;

namespace Zombles.UI
{
    public class UIMenu : UIWindow
    {
        private List<UIButton> _buttons;

        public int ButtonHeight;
        public int ButtonSpacing;
        public int ButtonMargin;

        public UIMenu( Vector2 size, float scale = 1.0f )
            : base( size, scale )
        {
            Title = "Menu";

            ButtonHeight = 32;
            ButtonSpacing = 8;
            ButtonMargin = 8;

            _buttons = new List<UIButton>();
        }

        public UIButton CreateButton( String text, MouseButtonEventHandler clickHandler = null )
        {
            return InsertButton( _buttons.Count, text, clickHandler );
        }

        public UIButton InsertButton( int index, String text, MouseButtonEventHandler clickHandler = null )
        {
            float y = ButtonMargin;
            if ( _buttons.Count != 0 && index != 0 )
                y = _buttons[ index - 1 ].Bottom + ButtonSpacing;

            for ( int i = index; i < _buttons.Count; ++i )
                _buttons[ i ].Top += _buttons[ i ].Height + ButtonSpacing;

            UIButton newButton = new UIButton( new Vector2( InnerWidth - ButtonMargin * 2, ButtonHeight ), new Vector2( ButtonMargin, y ) )
            {
                Text = text,
                CentreText = true
            };
            _buttons.Insert( index, newButton );
            AddChild( newButton );

            if ( clickHandler != null )
                newButton.Click += clickHandler;

            return newButton;
        }

        public void RemoveButton( UIButton button )
        {
            RemoveButton( _buttons.IndexOf( button ) );
        }

        public void RemoveButton( int index )
        {
            UIButton button = _buttons[ index ];
            _buttons.RemoveAt( index );
            RemoveChild( button );

            for ( int i = index; i < _buttons.Count; ++i )
                _buttons[ i ].Top -= _buttons[ i ].Height + ButtonSpacing;
        }

        public void AutoSize()
        {
            Height = _buttons.Count * ( ButtonHeight + ButtonSpacing ) - ButtonSpacing +
                ButtonMargin * 2 + PaddingTop + PaddingBottom;
        }
    }
}
