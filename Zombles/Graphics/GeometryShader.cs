using System;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class GeometryShader : ShaderProgram3D
    {
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

                    const float yscale = 1.0 / sqrt( 3.0 );

                    gl_Position = view_matrix * vec4(
                        in_vertex.x + world_offset.x,
                        float( ( dat >> 4 ) & 0xf ) * yscale,
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

            Create();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute( "in_vertex", 3 );

            AddTexture( "tiles", TextureUnit.Texture0 );
            SetTexture( "tiles", TextureManager.Tiles.TexArray );
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

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
