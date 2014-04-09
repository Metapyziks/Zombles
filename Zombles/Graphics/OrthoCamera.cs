using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;
using OpenTKTK.Scene;

namespace Zombles.Graphics
{
    public class OrthoCamera : Camera
    {
        private bool _offsetChanged;

        private Vector2 _worldOffset;
        private float _scale;

        private RectangleF _localViewBounds;
        private Rectangle _viewBounds;

        public int WrapWidth { get; private set; }
        public int WrapHeight { get; private set; }

        public Vector2 Position2D
        {
            get { return new Vector2(X, Z); }
            set
            {
                Position = new Vector3(value.X, Y, value.Y);
            }
        }

        public float Elevation
        {
            get { return Y; }
            private set { Y = value; }
        }

        public Vector2 WorldOffset
        {
            get { return _worldOffset; }
            set
            {
                _worldOffset = value;
                InvalidateViewBounds();
            }
        }

        public float WorldOffsetX
        {
            get { return _worldOffset.X; }
            set
            {
                _worldOffset.X = value;
                InvalidateViewBounds();
            }
        }

        public float WorldOffsetY
        {
            get { return _worldOffset.Y; }
            set
            {
                _worldOffset.Y = value;
                InvalidateViewBounds();
            }
        }

        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                InvalidateProjectionMatrix();
                InvalidateViewMatrix();
            }
        }

        public RectangleF LocalViewBounds
        {
            get
            {
                return _localViewBounds;
            }
        }

        public Rectangle ViewBounds
        {
            get
            {
                if (_offsetChanged) UpdateViewBounds();
                return _viewBounds;
            }
        }

        public OrthoCamera(int width, int height, float scale = 1.0f)
            : base(width, height)
        {
            Scale = scale;
        }

        public void SetWrapSize(int width, int height)
        {
            WrapWidth = width;
            WrapHeight = height;
        }

        public Vector2 ScreenToWorld(Vector2 pos, float height = 0.0f)
        {
            pos -= new Vector2(Width * 0.5f, Height * 0.5f);
            pos /= 8.0f * _scale;
            pos.Y /= (float) Math.Sin(Pitch);
            pos.Y += height * 4.0f * (float) (Math.Cos(Pitch) / Math.Sqrt(3.0));

            float sin = (float) Math.Sin(Yaw);
            float cos = (float) Math.Cos(Yaw);

            pos = new Vector2(
                X + cos * pos.X - sin * pos.Y,
                Z + sin * pos.X + cos * pos.Y
            );

            pos.X -= (int) Math.Floor(pos.X / WrapWidth) * WrapWidth;
            pos.Y -= (int) Math.Floor(pos.Y / WrapHeight) * WrapHeight;

            return pos;
        }

        protected override void OnUpdateProjectionMatrix(ref Matrix4 matrix)
        {
            float width = Width / (8.0f * _scale);
            float height = Height / (8.0f * _scale);

            double hoff = height * Math.Cos(Pitch);
            Y = (float) (hoff / 2.0) + 16.0f;
            float znear = 0.0f;
            float zfar = (float) ((hoff + 16.0f) / Math.Sin(Pitch)) + 1.0f;

            matrix = Matrix4.CreateOrthographic(
                width,
                height,
                znear, zfar
            );

            InvalidateViewMatrix();
        }

        protected override void OnUpdateViewMatrix(ref Matrix4 matrix)
        {
            UpdateLocalViewBounds();

            float rotOffset = (float) (Math.Tan(Math.PI / 2.0 - Pitch) * Y);

            Matrix4 yRot = Matrix4.CreateRotationY(Yaw);
            Matrix4 xRot = Matrix4.CreateRotationX(Pitch);
            Matrix4 trns = Matrix4.CreateTranslation(-Position);
            Matrix4 offs = Matrix4.CreateTranslation(0.0f, 0.0f, -rotOffset);

            matrix = Matrix4.Mult(Matrix4.Mult(Matrix4.Mult(trns, yRot), offs), xRot);
        }

        private void UpdateLocalViewBounds()
        {
            float width = Width / (8.0f * _scale);
            float height = Height / (8.0f * _scale);

            float xoff = width * 0.5f;
            float zoff = (float) (height / Math.Sin(Pitch) * 0.5);

            Vector2[] vs = new Vector2[]
            {
                new Vector2( -xoff, +zoff ), new Vector2( +xoff, +zoff ),
                new Vector2( -xoff, -zoff ), new Vector2( +xoff, -zoff )
            };

            float sin = (float) Math.Sin(Yaw);
            float cos = (float) Math.Cos(Yaw);

            float minx = float.MaxValue;
            float miny = float.MaxValue;
            float maxx = float.MinValue;
            float maxy = float.MinValue;

            for (int i = 0; i < 4; ++i) {
                Vector2 v = vs[i];
                v = new Vector2(X + cos * v.X - sin * v.Y,
                    Z + sin * v.X + cos * v.Y);

                if (v.X < minx)
                    minx = v.X;
                if (v.X > maxx)
                    maxx = v.X;
                if (v.Y < miny)
                    miny = v.Y;
                if (v.Y > maxy)
                    maxy = v.Y;
            }

            float hwid = (float) (WrapWidth >> 1);
            float hhei = (float) (WrapHeight >> 1);

            minx = Math.Max(X - hwid, minx);
            miny = Math.Max(Z - hhei, miny);
            maxx = Math.Min(X + hwid, maxx);
            maxy = Math.Min(Z + hhei, maxy);

            _localViewBounds = new RectangleF(minx, miny, maxx - minx, maxy - miny);

            InvalidateViewBounds();
        }

        public void InvalidateViewBounds()
        {
            _offsetChanged = true;
        }

        private void UpdateViewBounds()
        {
            _offsetChanged = false;

            int l = (int) Math.Floor(_localViewBounds.Left - _worldOffset.X);
            int t = (int) Math.Floor(_localViewBounds.Top - _worldOffset.Y);
            int r = (int) Math.Ceiling(_localViewBounds.Right - _worldOffset.X);
            int b = (int) Math.Ceiling(_localViewBounds.Bottom - _worldOffset.Y);

            _viewBounds = new Rectangle(l, t, r - l, b - t);
        }

        protected override void OnPositionChanged(PositionComponent component, ref Vector3 position)
        {
            if (component.HasFlag(PositionComponent.X) && WrapWidth > 0) {
                position.X -= (int) Math.Floor(position.X / WrapWidth) * WrapWidth;
            }

            if (component.HasFlag(PositionComponent.Z) && WrapHeight > 0) {
                position.Z -= (int) Math.Floor(position.Z / WrapHeight) * WrapHeight;
            }

            if (component.HasFlag(PositionComponent.Y)) {
                InvalidateProjectionMatrix();
            } else {
                base.OnPositionChanged(component, ref position);
            }
        }

        protected override void OnRotationChanged(RotationComponent component, ref Vector2 rotation)
        {
            if (component.HasFlag(RotationComponent.Pitch)) {
                InvalidateProjectionMatrix();
            } else {
                base.OnRotationChanged(component, ref rotation);
            }
        }
    }
}
