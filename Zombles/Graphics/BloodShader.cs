using System;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class BloodShader : ShaderProgram3D
    {
        private Vector2 myWorldSize;
        private int myWorldSizeLoc;

        public Vector2 WorldSize
        {
            get { return myWorldSize; }
            set
            {
                myWorldSize = value;
                GL.Uniform2( myWorldSizeLoc, ref value );
            }
        }

        public BloodShader()
        {
            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, false );
            vert.AddUniform( ShaderVarType.Mat4, "view_matrix" );
            vert.AddUniform( ShaderVarType.Vec2, "world_offset" );
            vert.AddUniform( ShaderVarType.Vec2, "world_size" );
            vert.AddAttribute( ShaderVarType.Vec2, "in_vertex" );
            vert.AddVarying( ShaderVarType.Vec2, "var_tex" );
            vert.Logic = @"
                void main( void )
                {
                    var_tex = vec2(
                        in_vertex.y / world_size.y,
                        in_vertex.x / world_size.x
                    );

                    gl_Position = view_matrix * vec4(
                        in_vertex.x + world_offset.x,
                        1.0 / 16.0,
                        in_vertex.y + world_offset.y,
                        1.0
                    );
                }
            ";

            ShaderBuilder frag = new ShaderBuilder( ShaderType.FragmentShader, false );
            frag.AddUniform( ShaderVarType.Sampler2D, "bloodmap" );
            frag.AddVarying( ShaderVarType.Vec2, "var_tex" );
            frag.Logic = @"
                void main( void )
                {
                    if( texture2DArray( bloodmap, var_tex ).a < 0.25 )
                        discard;

                    out_frag_colour = vec4( 0.6, 0.0, 0.0, 0.5 );
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

            myWorldSizeLoc = GL.GetUniformLocation( Program, "world_size" );

            AddAttribute( "in_vertex", 2 );

            AddTexture( "bloodmap", TextureUnit.Texture2 );
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Enable( EnableCap.DepthTest );
            GL.Enable( EnableCap.Blend );

            GL.BlendFunc( BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha );
        }

        protected override void OnEndBatch()
        {
            GL.Disable( EnableCap.DepthTest );
            GL.Disable( EnableCap.Blend );
        }
    }
}
