using System;

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
        private Matrix4 myPerspectiveMatrix;

        private bool myViewChanged;

        public Vector3 CameraPosition
        {
            get { return myCameraPosition; }
            set
            {
                myCameraPosition = value;
                myViewChanged = true;
            }
        }
        public Vector2 CameraRotation
        {
            get { return myCameraRotation; }
            set
            {
                myCameraRotation = value;
                myViewChanged = true;
            }
        }
        public Matrix4 PerspectiveMatrix
        {
            get { return myPerspectiveMatrix; }
            set
            {
                myPerspectiveMatrix = value;
                myViewChanged = true;
            }
        }

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
                    if( clr.a == 0.0 )
                        discard;
   
                    out_frag_colour = vec4( clr.rgb * var_shade, 1.0 );
                }
            ";

            VertexSource = vert.Generate( GL3 );
            FragmentSource = frag.Generate( GL3 );

            BeginMode = BeginMode.Quads;

            CameraPosition = new Vector3();
            CameraRotation = new Vector2();
        }

        public GeometryShader( int width, int height )
            : this()
        {
            Create();
            SetScreenSize( width, height );
        }

        public void SetScreenSize( int width, int height )
        {
            PerspectiveMatrix = Matrix4.CreateOrthographic(
                width / 32.0f,
                height / 32.0f,
                0.25f, 64.0f
            );
            UpdateViewMatrix();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute( "in_vertex", 3 );

            AddTexture( "tiles", TextureUnit.Texture0 );
            SetTexture( "tiles", TileManager.TexArray );

            myViewMatrixLoc = GL.GetUniformLocation( Program, "view_matrix" );
        }

        private void UpdateViewMatrix()
        {
            float rotOffset = (float) ( Math.Tan( Math.PI / 2.0 - CameraRotation.X ) * CameraPosition.Y );

            Matrix4 yRot = Matrix4.CreateRotationY( CameraRotation.Y );
            Matrix4 xRot = Matrix4.CreateRotationX( CameraRotation.X );
            Matrix4 trns = Matrix4.CreateTranslation( -CameraPosition );
            Matrix4 offs = Matrix4.CreateTranslation( 0.0f, 0.0f, -rotOffset );

            myViewMatrix = Matrix4.Mult( Matrix4.Mult( Matrix4.Mult( Matrix4.Mult( trns, yRot ), offs ), xRot ), PerspectiveMatrix );

            GL.UniformMatrix4( myViewMatrixLoc, false, ref myViewMatrix );

            myViewChanged = false;
        }

        protected override void OnStartBatch()
        {
            if ( myViewChanged )
                UpdateViewMatrix();

            GL.Enable( EnableCap.DepthTest );
            GL.Enable( EnableCap.CullFace );

            GL.CullFace( CullFaceMode.Front );
        }

        protected override void OnEndBatch()
        {
            GL.Disable( EnableCap.DepthTest );
            GL.Disable( EnableCap.CullFace );
        }
    }
}
