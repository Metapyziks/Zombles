using System;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class GeometryShader : ShaderProgram
    {
        private Matrix4 myViewMatrix;
        private Vector2 myWorldOffset;
        private int myViewMatrixLoc;
        private int myWorldOffsetLoc;

        private Vector3 myCameraPosition;
        private Vector2 myCameraRotation;
        private float myCameraScale;
        private Matrix4 myPerspectiveMatrix;
        private RectangleF myViewBounds;
        private Rectangle myOffsetViewBounds;

        private bool myPerspectiveChanged;
        private bool myViewChanged;
        private bool myOffsetChanged;

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public int WrapWidth { get; private set; }
        public int WrapHeight { get; private set; }

        public Vector2 CameraPosition
        {
            get
            {
                return new Vector2( myCameraPosition.X, myCameraPosition.Z );
            }
            set
            {
                CameraHorizontalPosition = value.X;
                CameraVerticalPosition = value.Y;
            }
        }
        public float CameraHorizontalPosition
        {
            get
            {
                return myCameraPosition.X;
            }
            set
            {
                if ( WrapWidth > 0 )
                    value -= (int) Math.Floor( value / WrapWidth ) * WrapWidth;

                myCameraPosition.X = value;
                myViewChanged = true;
            }
        }
        public float CameraVerticalPosition
        {
            get
            {
                return myCameraPosition.Z;
            }
            set
            {
                if ( WrapHeight > 0 )
                    value -= (int) Math.Floor( value / WrapHeight ) * WrapHeight;

                myCameraPosition.Z = value;
                myViewChanged = true;
            }
        }
        public float CameraPitch
        {
            get { return myCameraRotation.X; }
            set
            {
                if ( value != myCameraRotation.X )
                {
                    myCameraRotation.X = value;
                    myPerspectiveChanged = true;
                }
            }
        }
        public float CameraRotation
        {
            get { return myCameraRotation.Y; }
            set
            {
                myCameraRotation.Y = value;
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
        public float CameraScale
        {
            get { return myCameraScale; }
            set
            {
                myCameraScale = value;
                myPerspectiveChanged = true;
            }
        }
        public Matrix4 PerspectiveMatrix
        {
            get { return myPerspectiveMatrix; }
            set
            {
                myPerspectiveMatrix = value;
                myPerspectiveChanged = true;
            }
        }

        public Rectangle ViewBounds { get { return myOffsetViewBounds; } }

        public GeometryShader()
        {
            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, false );
            vert.AddUniform( ShaderVarType.Mat4, "view_matrix" );
            vert.AddUniform( ShaderVarType.Vec2, "world_offset" );
            vert.AddAttribute( ShaderVarType.Vec3, "in_vertex" );
            vert.AddVarying( ShaderVarType.Vec3, "var_tex" );
            vert.AddVarying( ShaderVarType.Float, "var_shade" );
            vert.Logic = @"
                void main( void )
                {
                    int dat = int( in_vertex.z );

                    var_tex = vec3(
                        float( dat & 0x1 ),
                        float( ( dat >> 1 ) & 0x3 ) / 2.0,
                        float( ( dat >> 8 ) & 0xffff )
                    );

                    var_shade = 1.0 - 0.125 * float( ( dat >> 3 ) & 0x1 );

                    gl_Position = view_matrix * vec4(
                        in_vertex.x + world_offset.x,
                        float( ( dat >> 4 ) & 0xf ) / 2.0,
                        in_vertex.y + world_offset.y,
                        1.0
                    );
                }
            ";

            ShaderBuilder frag = new ShaderBuilder( ShaderType.FragmentShader, false );
            frag.AddUniform( ShaderVarType.Sampler2DArray, "tiles" );
            frag.AddVarying( ShaderVarType.Vec3, "var_tex" );
            frag.AddVarying( ShaderVarType.Float, "var_shade" );
            frag.Logic = @"
                void main( void )
                {
                    vec4 clr = texture2DArray( tiles, var_tex );
                    if( clr.a < 1.0 )
                        discard;
   
                    out_frag_colour = vec4( clr.rgb * var_shade, 1.0 );
                }
            ";

            VertexSource = vert.Generate( GL3 );
            FragmentSource = frag.Generate( GL3 );

            BeginMode = BeginMode.Quads;

            myCameraPosition = new Vector3();
            myCameraRotation = new Vector2( MathHelper.Pi * 30.0f / 180.0f, 0.0f );
            myCameraScale = 1.0f;

            myPerspectiveChanged = true;
            myViewChanged = true;
            myOffsetChanged = true;
        }

        public GeometryShader( int width, int height )
            : this()
        {
            Create();
            SetScreenSize( width, height );
        }

        public void SetScreenSize( int width, int height )
        {
            ScreenWidth = width;
            ScreenHeight = height;
            UpdatePerspectiveMatrix();
        }

        public void SetWrapSize( int width, int height )
        {
            WrapWidth = width;
            WrapHeight = height;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute( "in_vertex", 3 );

            AddTexture( "tiles", TextureUnit.Texture0 );
            SetTexture( "tiles", TileManager.TexArray );

            myViewMatrixLoc = GL.GetUniformLocation( Program, "view_matrix" );
            myWorldOffsetLoc = GL.GetUniformLocation( Program, "world_offset" );
        }

        private void UpdatePerspectiveMatrix()
        {
            float width = ScreenWidth / ( 8.0f * myCameraScale );
            float height = ScreenHeight / ( 8.0f * myCameraScale );

            double hoff = height * Math.Cos( myCameraRotation.X );
            myCameraPosition.Y = (float) ( hoff / 2.0 ) + 16.0f;
            float znear = 0.0f;
            float zfar = (float) ( ( hoff + 16.0f ) / Math.Sin( myCameraRotation.X ) ) + 1.0f;

            myPerspectiveMatrix = Matrix4.CreateOrthographic(
                width,
                height,
                znear, zfar
            );
            UpdateViewMatrix();

            myPerspectiveChanged = false;
        }

        private void UpdateViewMatrix()
        {
            UpdateViewBounds();

            float rotOffset = (float) ( Math.Tan( Math.PI / 2.0 - myCameraRotation.X ) * myCameraPosition.Y );

            Matrix4 yRot = Matrix4.CreateRotationY( myCameraRotation.Y );
            Matrix4 xRot = Matrix4.CreateRotationX( myCameraRotation.X );
            Matrix4 trns = Matrix4.CreateTranslation( -myCameraPosition );
            Matrix4 offs = Matrix4.CreateTranslation( 0.0f, 0.0f, -rotOffset );

            myViewMatrix = Matrix4.Mult( Matrix4.Mult( Matrix4.Mult( Matrix4.Mult( trns, yRot ), offs ), xRot ), myPerspectiveMatrix );

            GL.UniformMatrix4( myViewMatrixLoc, false, ref myViewMatrix );

            myViewChanged = false;
        }

        private void UpdateWorldOffset()
        {
            GL.Uniform2( myWorldOffsetLoc, ref myWorldOffset );
            myOffsetChanged = false;
        }

        private void UpdateViewBounds()
        {
            float width = ScreenWidth / ( 8.0f * myCameraScale );
            float height = ScreenHeight / ( 8.0f * myCameraScale );

            float xoff = width * 0.5f;
            float zoff = (float) ( height / Math.Sin( myCameraRotation.X ) * 0.5 );

            Vector2[] vs = new Vector2[]
            {
                new Vector2( -xoff, +zoff ), new Vector2( +xoff, +zoff ),
                new Vector2( -xoff, -zoff ), new Vector2( +xoff, -zoff )
            };

            float sin = (float) Math.Sin( myCameraRotation.Y );
            float cos = (float) Math.Cos( myCameraRotation.Y );

            float minx = float.MaxValue;
            float miny = float.MaxValue;
            float maxx = float.MinValue;
            float maxy = float.MinValue;

            for ( int i = 0; i < 4; ++i )
            {
                Vector2 v = vs[ i ];
                v = new Vector2( myCameraPosition.X + cos * v.X - sin * v.Y,
                    myCameraPosition.Z + sin * v.X + cos * v.Y );
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

            minx = Math.Max( myCameraPosition.X - hwid, minx );
            miny = Math.Max( myCameraPosition.Z - hhei, miny );
            maxx = Math.Min( myCameraPosition.X + hwid, maxx );
            maxy = Math.Min( myCameraPosition.Z + hhei, maxy );

            myViewBounds = new RectangleF( minx, miny, maxx - minx, maxy - miny );
            UpdateViewBoundsOffset();
        }

        private void UpdateViewBoundsOffset()
        {
            int l = (int) Math.Floor( myViewBounds.Left - myWorldOffset.X );
            int t = (int) Math.Floor( myViewBounds.Top - myWorldOffset.Y );
            int r = (int) Math.Ceiling( myViewBounds.Right - myWorldOffset.X );
            int b = (int) Math.Ceiling( myViewBounds.Bottom - myWorldOffset.Y );

            myOffsetViewBounds = new Rectangle( l, t, r - l, b - t );
        }

        protected override void OnStartBatch()
        {
            if ( myPerspectiveChanged )
                UpdatePerspectiveMatrix();
            else if ( myViewChanged )
                UpdateViewMatrix();
            if ( myOffsetChanged )
            {
                UpdateWorldOffset();
                UpdateViewBoundsOffset();
            }

            GL.Enable( EnableCap.DepthTest );
            GL.Enable( EnableCap.CullFace );

            GL.CullFace( CullFaceMode.Front );
            GL.BlendFunc( BlendingFactorSrc.One, BlendingFactorDest.Zero );
        }

        protected override void OnEndBatch()
        {
            GL.Disable( EnableCap.DepthTest );
            GL.Disable( EnableCap.CullFace );
        }
    }
}
