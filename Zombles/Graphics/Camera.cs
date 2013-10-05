using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;

namespace Zombles.Graphics
{
    public class Camera
    {
        private bool _perspectiveChanged;
        private bool _viewChanged;
        private bool _offsetChanged;

        private Matrix4 _perspectiveMatrix;
        private Matrix4 _viewMatrix;
        private Vector2 _worldOffset;
        private Vector3 _position;
        private Vector2 _rotation;
        private float _scale;
        private RectangleF _viewBounds;
        private Rectangle _offsetViewBounds;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int WrapWidth { get; private set; }
        public int WrapHeight { get; private set; }

        public Matrix4 PerspectiveMatrix
        {
            get { return _perspectiveMatrix; }
            set
            {
                _perspectiveMatrix = value;
                _perspectiveChanged = false;
            }
        }

        public Matrix4 ViewMatrix
        {
            get { return _viewMatrix; }
            set
            {
                _viewMatrix = value;
                _viewChanged = false;
            }
        }

        public Vector2 Position
        {
            get { return new Vector2( _position.X, _position.Z ); }
            set
            {
                if ( WrapWidth > 0 )
                    value.X -= (int) Math.Floor( value.X / WrapWidth ) * WrapWidth;

                if ( WrapHeight > 0 )
                    value.Y -= (int) Math.Floor( value.Y / WrapHeight ) * WrapHeight;

                _position.X = value.X;
                _position.Z = value.Y;
                _viewChanged = true;
            }
        }

        public float Elevation
        {
            get { return _position.Y; }
            private set
            {
                _position.Y = value;
                _perspectiveChanged = true;
            }
        }

        public Vector2 Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _viewChanged = true;
            }
        }

        public float Pitch
        {
            get { return _rotation.X; }
            set
            {
                _rotation.X = value;
                _perspectiveChanged = true;
            }
        }

        public float Yaw
        {
            get { return _rotation.Y; }
            set
            {
                _rotation.Y = value;
                _viewChanged = true;
            }
        }

        public Vector2 WorldOffset
        {
            get { return _worldOffset; }
            set
            {
                _worldOffset = value;
                _offsetChanged = true;
            }
        }

        public float WorldHorizontalOffset
        {
            get { return _worldOffset.X; }
            set
            {
                _worldOffset.X = value;
                _offsetChanged = true;
            }
        }

        public float WorldVerticalOffset
        {
            get { return _worldOffset.Y; }
            set
            {
                _worldOffset.Y = value;
                _offsetChanged = true;
            }
        }

        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                _perspectiveChanged = true;
            }
        }

        public Rectangle ViewBounds { get { return _offsetViewBounds; } }

        public Camera( int width, int height, float scale = 1.0f )
        {
            Width = width;
            Height = height;

            Position = new Vector2();
            Rotation = new Vector2();

            Scale = scale;

            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        public void SetScreenSize( int width, int height )
        {
            Width = width;
            Height = height;
            UpdatePerspectiveMatrix();
        }

        public void SetWrapSize( int width, int height )
        {
            WrapWidth = width;
            WrapHeight = height;
        }

        public Vector2 ScreenToWorld( Vector2 pos, float height = 0.0f )
        {
            pos -= new Vector2( Width * 0.5f, Height * 0.5f );
            pos /= 8.0f * _scale;
            pos.Y /= (float) Math.Sin( _rotation.X );
            pos.Y += height * 4.0f * (float) ( Math.Cos( _rotation.X ) / Math.Sqrt( 3.0 ) );

            float sin = (float) Math.Sin( _rotation.Y );
            float cos = (float) Math.Cos( _rotation.Y );

            pos = new Vector2(
                _position.X + cos * pos.X - sin * pos.Y,
                _position.Z + sin * pos.X + cos * pos.Y
            );

            pos.X -= (int) Math.Floor( pos.X / WrapWidth  ) * WrapWidth;
            pos.Y -= (int) Math.Floor( pos.Y / WrapHeight ) * WrapHeight;

            return pos;
        }

        public void UpdatePerspectiveMatrix()
        {
            if ( _perspectiveChanged )
            {
                float width = Width / ( 8.0f * _scale );
                float height = Height / ( 8.0f * _scale );

                double hoff = height * Math.Cos( _rotation.X );
                _position.Y = (float) ( hoff / 2.0 ) + 16.0f;
                float znear = 0.0f;
                float zfar = (float) ( ( hoff + 16.0f ) / Math.Sin( _rotation.X ) ) + 1.0f;

                _perspectiveMatrix = Matrix4.CreateOrthographic(
                    width,
                    height,
                    znear, zfar
                );

                UpdateViewMatrix();
            }
        }

        public void UpdateViewMatrix()
        {
            if ( _viewChanged )
            {
                UpdateViewBounds();

                float rotOffset = (float) ( Math.Tan( Math.PI / 2.0 - _rotation.X ) * _position.Y );

                Matrix4 yRot = Matrix4.CreateRotationY( _rotation.Y );
                Matrix4 xRot = Matrix4.CreateRotationX( _rotation.X );
                Matrix4 trns = Matrix4.CreateTranslation( -_position );
                Matrix4 offs = Matrix4.CreateTranslation( 0.0f, 0.0f, -rotOffset );

                _viewMatrix = Matrix4.Mult( Matrix4.Mult( Matrix4.Mult( Matrix4.Mult( trns, yRot ), offs ), xRot ), _perspectiveMatrix );
            }
        }

        private void UpdateViewBounds()
        {
            float width = Width / ( 8.0f * _scale );
            float height = Height / ( 8.0f * _scale );

            float xoff = width * 0.5f;
            float zoff = (float) ( height / Math.Sin( _rotation.X ) * 0.5 );

            Vector2[] vs = new Vector2[]
            {
                new Vector2( -xoff, +zoff ), new Vector2( +xoff, +zoff ),
                new Vector2( -xoff, -zoff ), new Vector2( +xoff, -zoff )
            };

            float sin = (float) Math.Sin( _rotation.Y );
            float cos = (float) Math.Cos( _rotation.Y );

            float minx = float.MaxValue;
            float miny = float.MaxValue;
            float maxx = float.MinValue;
            float maxy = float.MinValue;

            for ( int i = 0; i < 4; ++i )
            {
                Vector2 v = vs[ i ];
                v = new Vector2( _position.X + cos * v.X - sin * v.Y,
                    _position.Z + sin * v.X + cos * v.Y );

                if ( v.X < minx )
                    minx = v.X;
                if ( v.X > maxx )
                    maxx = v.X;
                if ( v.Y < miny )
                    miny = v.Y;
                if ( v.Y > maxy )
                    maxy = v.Y;
            }

            float hwid = (float) ( WrapWidth >> 1 );
            float hhei = (float) ( WrapHeight >> 1 );

            minx = Math.Max( _position.X - hwid, minx );
            miny = Math.Max( _position.Z - hhei, miny );
            maxx = Math.Min( _position.X + hwid, maxx );
            maxy = Math.Min( _position.Z + hhei, maxy );

            _viewBounds = new RectangleF( minx, miny, maxx - minx, maxy - miny );
            _offsetChanged = true;
            UpdateViewBoundsOffset();
        }

        public void UpdateViewBoundsOffset()
        {
            if ( _offsetChanged )
            {
                int l = (int) Math.Floor( _viewBounds.Left - _worldOffset.X );
                int t = (int) Math.Floor( _viewBounds.Top - _worldOffset.Y );
                int r = (int) Math.Ceiling( _viewBounds.Right - _worldOffset.X );
                int b = (int) Math.Ceiling( _viewBounds.Bottom - _worldOffset.Y );

                _offsetViewBounds = new Rectangle( l, t, r - l, b - t );
                _offsetChanged = false;
            }
        }
    }
}
