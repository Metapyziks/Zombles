using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;

using Zombles.UI;

namespace Zombles.Graphics
{
    public class Scene : IDisposable
    {
        private UIObject myUIRoot;
        
        internal bool FirstTime;

        public ZomblesGame GameWindow { get; private set; }
        public SpriteShader SpriteShader
        {
            get { return GameWindow.SpriteShader; }
        }

        public int Width
        {
            get { return GameWindow.Width; }
        }

        public int Height
        {
            get { return GameWindow.Height; }
        }

        public System.Drawing.Rectangle Bounds
        {
            get { return GameWindow.Bounds; }
        }

        public bool IsCurrent
        {
            get { return GameWindow.CurrentScene == this; }
        }

        public KeyboardDevice Keyboard
        {
            get { return GameWindow.Keyboard; }
        }

        public MouseDevice Mouse
        {
            get { return GameWindow.Mouse; }
        }

        public Scene( ZomblesGame gameWindow )
        {
            GameWindow = gameWindow;
            myUIRoot = new UIObject( new Vector2( Width, Height ) );
            FirstTime = true;
        }

        public void AddChild( UIObject child )
        {
            myUIRoot.AddChild( child );
        }

        public void RemoveChild( UIObject child )
        {
            myUIRoot.RemoveChild( child );
        }

        public virtual void OnEnter( bool firstTime )
        {

        }

        public virtual void OnExit()
        {

        }

        public virtual void OnMouseButtonDown( MouseButtonEventArgs e )
        {
            myUIRoot.SendMouseButtonEvent( new Vector2( Mouse.X, Mouse.Y ), e );
        }

        public virtual void OnMouseButtonUp( MouseButtonEventArgs e )
        {
            myUIRoot.SendMouseButtonEvent( new Vector2( Mouse.X, Mouse.Y ), e );
        }

        public virtual void OnMouseMove( MouseMoveEventArgs e )
        {
            myUIRoot.SendMouseMoveEvent( new Vector2( Mouse.X, Mouse.Y ), e );
        }

        public virtual void OnMouseLeave( EventArgs e )
        {

        }

        public virtual void OnMouseEnter( EventArgs e )
        {

        }

        public virtual void OnMouseWheelChanged( MouseWheelEventArgs e )
        {

        }

        public virtual void OnKeyPress( KeyPressEventArgs e )
        {
            myUIRoot.SendKeyPressEvent( e );
        }

        public virtual void OnUpdateFrame( FrameEventArgs e )
        {

        }

        public virtual void OnRenderFrame( FrameEventArgs e )
        {
            SpriteShader.Begin();
            OnRenderSprites( e );
            SpriteShader.End();
        }

        public virtual void OnRenderSprites( FrameEventArgs e )
        {
            myUIRoot.Render( SpriteShader );
        }

        public virtual void Dispose()
        {

        }
    }
}
