using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;

using Zombles.UI;
using Zombles.Graphics;
using OpenTKTK.Shaders;

namespace Zombles
{
    public class Scene : IDisposable
    {
        private UIObject _uiRoot;

        internal bool FirstTime;

        public MainWindow GameWindow { get; private set; }
        public SpriteShader SpriteShader
        {
            get { return MainWindow.SpriteShader; }
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
            get { return MainWindow.CurrentScene == this; }
        }

        public KeyboardDevice Keyboard
        {
            get { return GameWindow.Keyboard; }
        }

        public MouseDevice Mouse
        {
            get { return GameWindow.Mouse; }
        }

        public Vector2 MousePos
        {
            get
            {
                return new Vector2(Mouse.X, Mouse.Y);
            }
        }

        public bool CursorVisible
        {
            get { return GameWindow.CursorVisible; }
            set { GameWindow.CursorVisible = value; }
        }

        public Scene(MainWindow gameWindow)
        {
            GameWindow = gameWindow;
            _uiRoot = new UIObject(new Vector2(Width, Height));
            FirstTime = true;
        }

        public void AddChild(UIObject child)
        {
            _uiRoot.AddChild(child);
        }

        public void RemoveChild(UIObject child)
        {
            _uiRoot.RemoveChild(child);
        }

        public virtual void OnEnter(bool firstTime)
        {

        }

        public virtual void OnExit()
        {

        }

        public event MouseButtonEventHandler MouseButtonDown;
        internal void TriggerMouseButtonDown(MouseButtonEventArgs e)
        {
            if (MouseButtonDown != null)
                MouseButtonDown(this, e);

            OnMouseButtonDown(e);
        }

        public virtual void OnMouseButtonDown(MouseButtonEventArgs e)
        {
            _uiRoot.SendMouseButtonEvent(new Vector2(Mouse.X, Mouse.Y), e);
        }

        public event MouseButtonEventHandler MouseButtonUp;
        internal void TriggerMouseButtonUp(MouseButtonEventArgs e)
        {
            if (MouseButtonUp != null)
                MouseButtonUp(this, e);

            OnMouseButtonUp(e);
        }

        public virtual void OnMouseButtonUp(MouseButtonEventArgs e)
        {
            _uiRoot.SendMouseButtonEvent(new Vector2(Mouse.X, Mouse.Y), e);
        }

        public event MouseMoveEventHandler MouseMove;
        internal void TriggerMouseMove(MouseMoveEventArgs e)
        {
            if (MouseMove != null)
                MouseMove(this, e);

            OnMouseMove(e);
        }

        public virtual void OnMouseMove(MouseMoveEventArgs e)
        {
            _uiRoot.SendMouseMoveEvent(new Vector2(Mouse.X, Mouse.Y), e);
        }

        public virtual void OnMouseLeave(EventArgs e)
        {

        }

        public virtual void OnMouseEnter(EventArgs e)
        {

        }

        public virtual void OnMouseWheelChanged(MouseWheelEventArgs e)
        {

        }

        public virtual void OnKeyPress(KeyboardKeyEventArgs e)
        {
            _uiRoot.SendKeyPressEvent(e);
        }

        public virtual void OnUpdateFrame(FrameEventArgs e)
        {

        }

        public virtual void OnRenderFrame(FrameEventArgs e)
        {
            SpriteShader.Begin(true);
            OnRenderSprites(e);
            SpriteShader.End();
        }

        public virtual void OnRenderSprites(FrameEventArgs e)
        {
            _uiRoot.Render(SpriteShader);
        }

        public virtual void OnResize()
        {

        }

        public virtual void Dispose()
        {

        }
    }
}
