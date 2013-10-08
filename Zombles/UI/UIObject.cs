using System;
using System.Collections.Generic;
using System.Drawing;

using OpenTK;

using OpenTKTK.Textures;

using ResourceLibrary;

using Zombles.Graphics;

namespace Zombles.UI
{
    public class ResizeEventArgs : EventArgs
    {
        public readonly Vector2 Size;

        public ResizeEventArgs(Vector2 size)
        {
            Size = size;
        }
    }

    public delegate void ResizeEventHandler(Object sender, ResizeEventArgs e);

    public class RepositionEventArgs : EventArgs
    {
        public readonly Vector2 Position;

        public RepositionEventArgs(Vector2 position)
        {
            Position = position;
        }
    }

    public delegate void RepositionEventHandler(Object sender, RepositionEventArgs e);

    public class VisibilityChangedEventArgs : EventArgs
    {
        public readonly bool Visible;
        public bool Hidden
        {
            get
            {
                return !Visible;
            }
        }

        public VisibilityChangedEventArgs(bool visible)
        {
            Visible = visible;
        }
    }

    public delegate void VisibilityChangedEventHandler(Object sender, VisibilityChangedEventArgs e);

    public class EnabledChangedEventArgs : EventArgs
    {
        public readonly bool Enabled;
        public bool Disabled
        {
            get
            {
                return !Enabled;
            }
        }

        public EnabledChangedEventArgs(bool enabled)
        {
            Enabled = enabled;
        }
    }

    public delegate void EnabledChangedEventHandler(Object sender, EnabledChangedEventArgs e);
    public delegate void MouseButtonEventHandler(Object sender, OpenTK.Input.MouseButtonEventArgs e);
    public delegate void MouseMoveEventHandler(Object sender, OpenTK.Input.MouseMoveEventArgs e);
    public delegate void KeyPressEventHandler(Object sender, KeyPressEventArgs e);

    public class RenderEventArgs : EventArgs
    {
        public readonly SpriteShader ShaderProgram;
        public readonly Vector2 DrawPosition;

        public RenderEventArgs(SpriteShader shader, Vector2 drawPosition)
        {
            ShaderProgram = shader;
            DrawPosition = drawPosition;
        }
    }

    public delegate void RenderEventHandler(Object sender, RenderEventArgs e);

    public class UIObject
    {
        private static BitmapTexture2D _sPanelsTexture;

        protected static BitmapTexture2D PanelsTexture
        {
            get
            {
                if (_sPanelsTexture == null) {
                    _sPanelsTexture = new BitmapTexture2D(Archive.Get<Bitmap>("images", "gui", "panels"));
                }

                return _sPanelsTexture;
            }
        }

        private Vector2 _size;
        private Vector2 _position;
        private Vector2 _paddingTopLeft;
        private Vector2 _paddingBottomRight;
        private bool _visible;
        private bool _enabled;
        private bool _focused;
        private List<UIObject> _children;
        private Vector2 _mousePos;
        private bool _mouseOver;
        private bool _mouseDown;
        private UIObject _parent;

        protected bool CanReposition;
        protected bool CanResize;
        protected bool CanBringToFront;

        public OpenTK.Graphics.Color4 DisabledColour = new OpenTK.Graphics.Color4(95, 95, 95, 255);

        public Vector2 Size
        {
            get
            {
                return _size;
            }
            set
            {
                SetSize(value);
            }
        }

        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                SetPosition(value);
            }
        }

        public float Left
        {
            get
            {
                return _position.X;
            }
            set
            {
                SetPosition(value, _position.Y);
            }
        }

        public float Top
        {
            get
            {
                return _position.Y;
            }
            set
            {
                SetPosition(_position.X, value);
            }
        }

        public float Right
        {
            get
            {
                return _position.X + _size.X;
            }
            set
            {
                SetPosition(value - _size.X, _position.Y);
            }
        }

        public float Bottom
        {
            get
            {
                return _position.Y + _size.Y;
            }
            set
            {
                SetPosition(_position.X, value - _size.Y);
            }
        }

        public float Width
        {
            get
            {
                return _size.X;
            }
            set
            {
                SetSize(value, _size.Y);
            }
        }

        public float Height
        {
            get
            {
                return _size.Y;
            }
            set
            {
                SetSize(_size.X, value);
            }
        }

        public float PaddingLeft
        {
            get
            {
                return _paddingTopLeft.X;
            }
            set
            {
                _paddingTopLeft.X = value;
            }
        }

        public float PaddingTop
        {
            get
            {
                return _paddingTopLeft.Y;
            }
            set
            {
                _paddingTopLeft.Y = value;
            }
        }

        public float PaddingRight
        {
            get
            {
                return _paddingBottomRight.X;
            }
            set
            {
                _paddingBottomRight.X = value;
            }
        }

        public float PaddingBottom
        {
            get
            {
                return _paddingBottomRight.Y;
            }
            set
            {
                _paddingBottomRight.Y = value;
            }
        }

        public float InnerWidth
        {
            get
            {
                return _size.X - _paddingTopLeft.X - _paddingBottomRight.X;
            }
        }

        public float InnerHeight
        {
            get
            {
                return _size.Y - _paddingTopLeft.Y - _paddingBottomRight.Y;
            }
        }

        public bool IsVisible
        {
            get
            {
                return _visible && (Parent == null || Parent.IsVisible);
            }
            set
            {
                if (value)
                    Show();
                else
                    Hide();
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _enabled && (Parent == null || Parent.IsEnabled);
            }
            set
            {
                if (value)
                    Enable();
                else
                    Disable();
            }
        }

        public bool IsFocused
        {
            get
            {
                return _focused;
            }
        }

        public Vector2 MousePosition
        {
            get
            {
                return _mousePos;
            }
        }

        public bool MouseOver
        {
            get
            {
                return _mouseOver;
            }
        }

        public bool MouseButtonPressed
        {
            get
            {
                return _mouseDown;
            }
        }

        public UIObject Parent
        {
            get
            {
                return _parent;
            }
        }

        public UIObject[] Children
        {
            get
            {
                return _children.ToArray();
            }
        }

        public UIObject()
            : this(new Vector2(), new Vector2())
        {

        }

        public UIObject(Vector2 size)
            : this(size, new Vector2())
        {

        }

        public UIObject(Vector2 size, Vector2 position)
        {
            _size = size;
            _position = position;
            _visible = true;
            _enabled = true;
            _focused = false;
            _mouseOver = false;
            _mouseDown = false;

            _children = new List<UIObject>();

            CanReposition = true;
            CanResize = true;
            CanBringToFront = false;
        }

        public event ResizeEventHandler Resize;

        protected virtual Vector2 OnSetSize(Vector2 newSize)
        {
            return newSize;
        }

        public event RepositionEventHandler Reposition;

        protected virtual Vector2 OnSetPosition(Vector2 newPosition)
        {
            return newPosition;
        }

        public event EventHandler Focused;

        protected virtual void OnFocus()
        {

        }

        public event EventHandler UnFocused;

        protected virtual void OnUnFocus()
        {

        }

        public event VisibilityChangedEventHandler VisibilityChanged;
        public event EventHandler Shown;

        protected virtual void OnShow()
        {

        }

        public event EventHandler Hidden;

        protected virtual void OnHide()
        {

        }

        public event EnabledChangedEventHandler EnabledChanged;
        public event EventHandler Enabled;

        protected virtual void OnEnable()
        {

        }

        public event EventHandler Disabled;

        protected virtual void OnDisable()
        {

        }

        public event MouseButtonEventHandler MouseDown;

        protected virtual void OnMouseDown(Vector2 mousePos, OpenTK.Input.MouseButton mouseButton)
        {

        }

        public event MouseButtonEventHandler MouseUp;

        protected virtual void OnMouseUp(Vector2 mousePos, OpenTK.Input.MouseButton mouseButton)
        {

        }

        public event MouseButtonEventHandler Click;

        protected virtual void OnClick(Vector2 mousePos, OpenTK.Input.MouseButton mouseButton)
        {

        }

        public event MouseMoveEventHandler MouseMove;

        protected virtual void OnMouseMove(Vector2 mousePos)
        {

        }

        public event MouseMoveEventHandler MouseEnter;

        protected virtual void OnMouseEnter(Vector2 mousePos)
        {

        }

        public event MouseMoveEventHandler MouseLeave;

        protected virtual void OnMouseLeave(Vector2 mousePos)
        {

        }

        public event KeyPressEventHandler KeyPress;

        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {

        }

        public event RenderEventHandler RenderObject;

        protected virtual void OnRender(SpriteShader shader, Vector2 renderPosition = new Vector2())
        {

        }

        protected virtual bool CheckPositionWithinBounds(Vector2 pos)
        {
            return IsVisible &&
                pos.X >= 0 &&
                pos.Y >= 0 &&
                pos.X < Size.X &&
                pos.Y < Size.Y;
        }

        public void SetSize(float width, float height)
        {
            SetSize(new Vector2(width, height));
        }

        public void SetSize(Vector2 size)
        {
            if (CanResize) {
                _size = OnSetSize(size);
                if (Resize != null)
                    Resize(this, new ResizeEventArgs(size));
            }
        }

        public void SetPosition(float x, float y)
        {
            SetPosition(new Vector2(x, y));
        }

        public void SetPosition(Vector2 position)
        {
            if (CanReposition) {
                _position = OnSetPosition(position);
                if (Reposition != null)
                    Reposition(this, new RepositionEventArgs(position));
            }
        }

        public void CentreHorizontally()
        {
            if (Parent != null)
                Left = (Parent.InnerWidth - Width) / 2.0f;
        }

        public void CentreVertically()
        {
            if (Parent != null)
                Top = (Parent.InnerHeight - Height) / 2.0f;
        }

        public void Centre()
        {
            if (Parent != null)
                Position = new Vector2(Parent.InnerWidth - Width, Parent.InnerHeight - Height) / 2.0f;
        }

        public void Focus()
        {
            _focused = true;

            if (Parent != null) {
                foreach (UIObject child in Parent._children)
                    if (child.IsFocused && child != this)
                        child.UnFocus();
            }

            OnFocus();
            if (Focused != null)
                Focused(this, new EventArgs());
        }

        public void UnFocus()
        {
            _focused = false;

            foreach (UIObject child in _children)
                if (child.IsFocused)
                    child.UnFocus();

            OnUnFocus();
            if (UnFocused != null)
                UnFocused(this, new EventArgs());
        }

        public void Show()
        {
            if (!_visible) {
                OnShow();
                if (VisibilityChanged != null)
                    VisibilityChanged(this, new VisibilityChangedEventArgs(true));
                if (Shown != null)
                    Shown(this, new EventArgs());
            }
            _visible = true;
        }

        public void Hide()
        {
            if (_focused)
                UnFocus();

            if (_visible) {
                OnHide();
                if (VisibilityChanged != null)
                    VisibilityChanged(this, new VisibilityChangedEventArgs(false));
                if (Hidden != null)
                    Hidden(this, new EventArgs());
            }
            _visible = false;
        }

        public void Enable()
        {
            if (!_enabled) {
                OnEnable();
                if (EnabledChanged != null)
                    EnabledChanged(this, new EnabledChangedEventArgs(true));
                if (Enabled != null)
                    Enabled(this, new EventArgs());
            }
            _enabled = true;
        }

        public void Disable()
        {
            if (_enabled) {
                _mouseDown = false;
                _mouseOver = false;

                OnDisable();
                if (EnabledChanged != null)
                    EnabledChanged(this, new EnabledChangedEventArgs(false));
                if (Disabled != null)
                    Disabled(this, new EventArgs());
            }
            _enabled = false;
        }

        public UIObject GetFirstIntersector(Vector2 pos)
        {
            if (_children.Count > 0) {
                UIObject intersector = null;

                for (int i = _children.Count - 1; i >= 0; --i) {
                    UIObject child = _children[i];

                    if (child.IsVisible && (intersector = child.GetFirstIntersector(pos - _paddingTopLeft - child.Position)) != null)
                        return intersector;
                }
            }

            if (CheckPositionWithinBounds(pos))
                return this;

            return null;
        }

        public void SendMouseButtonEvent(Vector2 mousePos, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (e.IsPressed) {
                if (_children.Count > 0) {
                    UIObject intersector = null;

                    for (int i = _children.Count - 1; i >= 0; --i) {
                        UIObject child = _children[i];

                        Vector2 relativePos = mousePos - _paddingTopLeft - child.Position;

                        if (child.IsVisible && (intersector = child.GetFirstIntersector(relativePos)) != null) {
                            if (child.IsEnabled) {
                                if (child.CanBringToFront) {
                                    _children.Remove(child);
                                    _children.Add(child);
                                }

                                child.SendMouseButtonEvent(relativePos, e);
                            }

                            if (IsEnabled) {
                                Focus();

                                if (!child.IsEnabled) {
                                    foreach (UIObject ch in _children)
                                        if (ch.IsFocused)
                                            ch.UnFocus();

                                    _mouseDown = true;
                                    OnMouseDown(mousePos, e.Button);
                                    if (MouseDown != null)
                                        MouseDown(this, e);
                                }
                            }
                            return;
                        }
                    }
                }

                if (CheckPositionWithinBounds(mousePos)) {
                    if (IsEnabled) {
                        Focus();

                        foreach (UIObject ch in _children)
                            if (ch.IsFocused)
                                ch.UnFocus();

                        _mouseDown = true;
                        OnMouseDown(mousePos, e.Button);
                        if (MouseDown != null)
                            MouseDown(this, e);
                    }
                }
            } else {
                UIObject intersector = null;

                if (IsVisible && (intersector = GetFirstIntersector(mousePos)) != null) {
                    OnMouseUp(mousePos, e.Button);
                    if (MouseUp != null)
                        MouseUp(this, e);
                }

                if (_mouseDown) {
                    _mouseDown = false;

                    if (IsVisible && intersector != null) {
                        OnClick(mousePos, e.Button);

                        if (Click != null)
                            Click(this, e);
                    }
                } else {
                    if (_children.Count > 0) {
                        for (int i = _children.Count - 1; i >= 0 && i < _children.Count; --i) {
                            UIObject child = _children[i];

                            Vector2 relativePos = mousePos - _paddingTopLeft - child.Position;

                            if (child.IsEnabled)
                                child.SendMouseButtonEvent(relativePos, e);
                        }
                    }
                }
            }
        }

        public void SendMouseMoveEvent(Vector2 newPos, OpenTK.Input.MouseMoveEventArgs e)
        {
            if (IsEnabled && IsVisible && newPos != _mousePos) {
                OnMouseMove(newPos);
                if (MouseMove != null)
                    MouseMove(this, e);
            }

            _mousePos = newPos;

            bool mouseNowOver = CheckPositionWithinBounds(newPos);
            if (IsEnabled && IsVisible && mouseNowOver != _mouseOver) {
                _mouseOver = mouseNowOver;

                if (_mouseOver) {
                    OnMouseEnter(_mousePos);
                    if (MouseEnter != null)
                        MouseEnter(this, e);
                } else {
                    OnMouseLeave(_mousePos);
                    if (MouseLeave != null)
                        MouseLeave(this, e);
                }
            }

            for (int i = _children.Count - 1; i >= 0; --i)
                _children[i].SendMouseMoveEvent(newPos - _paddingTopLeft - _children[i].Position, e);
        }

        public void SendKeyPressEvent(KeyPressEventArgs e)
        {
            if (IsFocused && IsEnabled) {
                OnKeyPress(e);
                if (KeyPress != null)
                    KeyPress(this, e);

                foreach (UIObject child in _children)
                    if (child.IsFocused && IsEnabled) {
                        child.SendKeyPressEvent(e);
                        break;
                    }
            }
        }

        public void AddChild(UIObject child)
        {
            if (child._parent != null)
                child._parent.RemoveChild(child);

            _children.Add(child);
            child._parent = this;

            if (child is UIWindow) {
                (child as UIWindow).Closed += delegate(object sender, EventArgs e) {
                    RemoveChild(sender as UIWindow);
                };
            }
        }

        public void RemoveChild(UIObject child)
        {
            if (_children.Contains(child)) {
                _children.Remove(child);
                child._parent = null;
            }
        }

        public void Render(SpriteShader shader, Vector2 parentPosition = new Vector2())
        {
            if (IsVisible) {
                parentPosition += Position;

                OnRender(shader, parentPosition);
                if (RenderObject != null)
                    RenderObject(this, new RenderEventArgs(shader, parentPosition));

                foreach (UIObject child in _children)
                    child.Render(shader, parentPosition + _paddingTopLeft);
            }
        }
    }
}
