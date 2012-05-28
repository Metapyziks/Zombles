using System;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class FlatEntityShader : ShaderProgram3D
    {
        private static VertexBuffer stVB;

        private int myScaleLoc;
        private int myPositionLoc;
        private int mySizeLoc;

        public FlatEntityShader()
        {
            if ( stVB == null )
            {
                stVB = new VertexBuffer( 3 );
                stVB.SetData( new float[]
                {
                    -0.5f, 1.0f, 0.0f,
                    0.5f, 1.0f, 1.0f,
                    0.5f, 0.0f, 3.0f,
                    -0.5f, 0.0f, 2.0f
                } );
            }

            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, false );
            vert.AddUniform( ShaderVarType.Mat4, "view_matrix" );
            vert.AddUniform( ShaderVarType.Vec2, "world_offset" );
            vert.AddUniform( ShaderVarType.Vec2, "scale" );
            vert.AddUniform( ShaderVarType.Vec3, "position" );
            vert.AddUniform( ShaderVarType.Vec2, "size" );
            vert.AddAttribute( ShaderVarType.Vec3, "in_vertex" );
            vert.AddVarying( ShaderVarType.Vec3, "var_colour" );
            vert.Logic = @"
                void main( void )
                {
                    var_colour = vec3( 1.0, 0.0, 0.0 );

                    gl_Position = view_matrix * vec4(
                        position.x + world_offset.x,
                        position.y,
                        position.z + world_offset.y,
                        1.0
                    ) + vec4( in_vertex.xy * scale * size, 0.0, 0.0 );
                }
            ";

            ShaderBuilder frag = new ShaderBuilder( ShaderType.FragmentShader, false );
            frag.AddVarying( ShaderVarType.Vec3, "var_colour" );
            frag.Logic = @"
                void main( void )
                {
                    out_frag_colour = vec4( var_colour, 1.0 );
                }
            ";

            VertexSource = vert.Generate( GL3 );
            FragmentSource = frag.Generate( GL3 );

            BeginMode = BeginMode.Quads;

            Create();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute( "in_vertex", 3 );

            myScaleLoc = GL.GetUniformLocation( Program, "scale" );
            myPositionLoc = GL.GetUniformLocation( Program, "position" );
            mySizeLoc = GL.GetUniformLocation( Program, "size" );
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Uniform2( myScaleLoc, 16.0f / Camera.Width * Camera.Scale, 16.0f / Camera.Height * Camera.Scale );

            GL.Enable( EnableCap.DepthTest );

            GL.BlendFunc( BlendingFactorSrc.One, BlendingFactorDest.Zero );

            stVB.StartBatch( this );
        }

        public void Render( Vector3 pos, Vector2 size )
        {
            GL.Uniform3( myPositionLoc, ref pos );
            GL.Uniform2( mySizeLoc, ref size );
            stVB.Render( this );
        }

        protected override void OnEndBatch()
        {
            stVB.EndBatch( this );

            GL.Disable( EnableCap.DepthTest );
        }
    }
}
