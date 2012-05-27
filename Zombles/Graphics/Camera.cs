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
        private bool myPerspectiveChanged;
        private bool myViewChanged;
        private bool myOffsetChanged;

        private Matrix4 myPerspectiveMatrix;
        private Matrix4 myViewMatrix;
        private Vector2 myWorldOffset;
        private Vector3 myPosition;
        private Vector2 myRotation;
        private float myScale;
        private RectangleF myViewBounds;
        private Rectangle myOffsetViewBounds;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int WrapWidth { get; private set; }
        public int WrapHeight { get; private set; }

        public Matrix4 PerspectiveMatrix
        {
            get { return myPerspectiveMatrix; }
            set
            {
                myPerspectiveMatrix = value;
                myPerspectiveChanged = false;
            }
        }

        public Matrix4 ViewMatrix
        {
            get { return myViewMatrix; }
            set
            {
                myViewMatrix = value;
                myViewChanged = false;
            }
        }

        public Vector2 Position
        {
            get { return new Vector2( myPosition.X, myPosition.Z ); }
            set
            {
                if ( WrapWidth > 0 )
                    value.X -= (int) Math.Floor( value.X / WrapWidth ) * WrapWidth;

                if ( WrapHeight > 0 )
                    value.Y -= (int) Math.Floor( value.Y / WrapHeight ) * WrapHeight;

                myPosition.X = value.X;
                myPosition.Z = value.Y;
                myViewChanged = true;
            }
        }

        public float Elevation
        {
            get { return myPosition.Y; }
            private set
            {
                myPosition.Y = value;
                myPerspectiveChanged = true;
            }
        }

        public Vector2 Rotation
        {
            get { return myRotation; }
            set
            {
                myRotation = value;
                myViewChanged = true;
            }
        }

        public float Pitch
        {
            get { return myRotation.X; }
            set
            {
                myRotation.X = value;
                myPerspectiveChanged = true;
            }
        }

        public float Yaw
        {
            get { return myRotation.Y; }
            set
            {
                myRotation.Y = value;
                myViewChanged = true;
            }
        }

        public Vector2 WorldOffset
        {
            get { return myWorldOffset; }
            set
            {
                myWorldOffset = value;
                myOffsetChanged = true;
            }
        }

        public float WorldHorizontalOffset
        {
            get { return myWorldOffset.X; }
            set
            {
                myWorldOffset.X = value;
                myOffsetChanged = true;
            }
        }

        public float WorldVerticalOffset
        {
            get { return myWorldOffset.Y; }
            set
            {
                myWorldOffset.Y = value;
                myOffsetChanged = true;
            }
        }

        public float Scale
        {
            get { return myScale; }
            set
            {
                myScale = value;
                myPerspectiveChanged = true;
            }
        }

        public Rectangle ViewBounds { get { return myOffsetViewBounds; } }

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

        public void UpdatePerspectiveMatrix()
        {
            if ( myPerspectiveChanged )
            {
                float width = Width / ( 8.0f * myScale );
                float height = Height / ( 8.0f * myScale );

                double hoff = height * Math.Cos( myRotation.X );
                myPosition.Y = (float) ( hoff / 2.0 ) + 16.0f;
                float znear = 0.0f;
                float zfar = (float) ( ( hoff + 16.0f ) / Math.Sin( myRotation.X ) ) + 1.0f;

                myPerspectiveMatrix = Matrix4.CreateOrthographic(
                    width,
                    height,
                    znear, zfar
                );

                UpdateViewMatrix();
            }
        }

        public void UpdateViewMatrix()
        {
            if ( myViewChanged )
            {
                UpdateViewBounds();

                float rotOffset = (float) ( Math.Tan( Math.PI / 2.0 - myRotation.X ) * myPosition.Y );

                Matrix4 yRot = Matrix4.CreateRotationY( myRotation.Y );
                Matrix4 xRot = Matrix4.CreateRotationX( myRotation.X );
                Matrix4 trns = Matrix4.CreateTranslation( -myPosition );
                Matrix4 offs = Matrix4.CreateTranslation( 0.0f, 0.0f, -rotOffset );

                myViewMatrix = Matrix4.Mult( Matrix4.Mult( Matrix4.Mult( Matrix4.Mult( trns, yRot ), offs ), xRot ), myPerspectiveMatrix );
            }
        }

        private void UpdateViewBounds()
        {
            float width = Width / ( 8.0f * myScale );
            float height = Height / ( 8.0f * myScale );

            float xoff = width * 0.5f;
            float zoff = (float) ( height / Math.Sin( myRotation.X ) * 0.5 );

            Vector2[] vs = new Vector2[]
            {
                new Vector2( -xoff, +zoff ), new Vector2( +xoff, +zoff ),
                new Vector2( -xoff, -zoff ), new Vector2( +xoff, -zoff )
            };

            float sin = (float) Math.Sin( myRotation.Y );
            float cos = (float) Math.Cos( myRotation.Y );

            float minx = float.MaxValue;
            float miny = float.MaxValue;
            float maxx = float.MinValue;
            float maxy = float.MinValue;

            for ( int i = 0; i < 4; ++i )
            {
                Vector2 v = vs[ i ];
                v = new Vector2( myPosition.X + cos * v.X - sin * v.Y,
                    myPosition.Z + sin * v.X + cos * v.Y );

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

            minx = Math.Max( myPosition.X - hwid, minx );
            miny = Math.Max( myPosition.Z - hhei, miny );
            maxx = Math.Min( myPosition.X + hwid, maxx );
            maxy = Math.Min( myPosition.Z + hhei, maxy );

            myViewBounds = new RectangleF( minx, miny, maxx - minx, maxy - miny );
            myOffsetChanged = true;
            UpdateViewBoundsOffset();
        }

        public void UpdateViewBoundsOffset()
        {
            if ( myOffsetChanged )
            {
                int l = (int) Math.Floor( myViewBounds.Left - myWorldOffset.X );
                int t = (int) Math.Floor( myViewBounds.Top - myWorldOffset.Y );
                int r = (int) Math.Ceiling( myViewBounds.Right - myWorldOffset.X );
                int b = (int) Math.Ceiling( myViewBounds.Bottom - myWorldOffset.Y );

                myOffsetViewBounds = new Rectangle( l, t, r - l, b - t );
                myOffsetChanged = false;
            }
        }
    }
}
