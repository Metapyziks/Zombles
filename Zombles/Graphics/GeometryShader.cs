using System;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class GeometryShader : ShaderProgram
    {
        private Matrix4 myViewMatrix;
        private int myViewMatrixLoc;

        private Vector3 myCameraPosition;
        private Vector2 myCameraRotation;
        private float myCameraScale;
        private Matrix4 myPerspectiveMatrix;

        private bool myPerspectiveChanged;
        private bool myViewChanged;

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public Vector2 CameraPosition
        {
            get
            {
                return new Vector2( myCameraPosition.X, myCameraPosition.Z );
            }
            set
            {
                myCameraPosition.X = value.X;
                myCameraPosition.Z = value.Y;
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

        public Rectangle ViewBounds { get; private set; }

        public GeometryShader()
        {
            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, false );
            vert.AddUniform( ShaderVarType.Mat4, "view_matrix" );
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
                        in_vertex.x,
                        float( ( dat >> 4 ) & 0xf ) / 2.0,
                        in_vertex.y,
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

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute( "in_vertex", 3 );

            AddTexture( "tiles", TextureUnit.Texture0 );
            SetTexture( "tiles", TileManager.TexArray );

            myViewMatrixLoc = GL.GetUniformLocation( Program, "view_matrix" );
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

            int iminx = (int) Math.Floor( minx );
            int iminy = (int) Math.Floor( miny );
            int imaxx = (int) Math.Ceiling( maxx );
            int imaxy = (int) Math.Ceiling( maxy );

            ViewBounds = new Rectangle( iminx, iminy, imaxx - iminx, imaxy - iminy );
        }        

        protected override void OnStartBatch()
        {
            if ( myPerspectiveChanged )
                UpdatePerspectiveMatrix();
            else if ( myViewChanged )
                UpdateViewMatrix();

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
