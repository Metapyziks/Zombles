using System;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using OpenTKTK.Shaders;
using OpenTKTK.Utils;

namespace Zombles.Graphics
{
    public class FlatEntityShader : ShaderProgram3D<OrthoCamera>
    {
        private static VertexBuffer _sVB;

        private int _scaleLoc;
        private int _positionLoc;
        private int _sizeLoc;
        private int _textureLoc;

        public FlatEntityShader()
        {
            if (_sVB == null) {
                _sVB = new VertexBuffer(3);
                _sVB.SetData(new float[]
                {
                    -0.5f, 1.0f, 0.0f,
                    0.5f, 1.0f, 1.0f,
                    0.5f, 0.0f, 3.0f,
                    -0.5f, 0.0f, 2.0f
                });
            }

            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "view_matrix");
            vert.AddUniform(ShaderVarType.Vec2, "world_offset");
            vert.AddUniform(ShaderVarType.Vec2, "scale");
            vert.AddUniform(ShaderVarType.Vec3, "position");
            vert.AddUniform(ShaderVarType.Vec2, "size");
            vert.AddUniform(ShaderVarType.Float, "texture");
            vert.AddAttribute(ShaderVarType.Vec3, "in_vertex");
            vert.AddVarying(ShaderVarType.Vec3, "var_tex");
            vert.Logic = @"
                void main( void )
                {
                    switch( int( in_vertex.z ) )
                    {
                        case 0:
                            var_tex = vec3( 0.0, 0.0, texture ); break;
                        case 1:
                            var_tex = vec3( size.x, 0.0, texture ); break;
                        case 2:
                            var_tex = vec3( 0.0, size.y, texture ); break;
                        case 3:
                            var_tex = vec3( size.x, size.y, texture ); break;
                    }

                    const float yscale = 2.0 / sqrt( 3.0 );

                    gl_Position = view_matrix * vec4(
                        position.x + world_offset.x,
                        ( position.y + in_vertex.y * size.y ) * yscale,
                        position.z + world_offset.y,
                        1.0
                    ) + vec4( in_vertex.x * scale.x * size.x, 0.0, 0.0, 0.0 );
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false);
            frag.AddUniform(ShaderVarType.Sampler2DArray, "ents");
            frag.AddVarying(ShaderVarType.Vec3, "var_tex");
            frag.FragOutIdentifier = "out_frag_colour";
            frag.Logic = @"
                void main( void )
                {
                    out_frag_colour = texture2DArray( ents, var_tex );
                    if( out_frag_colour.a < 0.5 )
                        discard;
                }
            ";

            VertexSource = vert.Generate(GL3);
            FragmentSource = frag.Generate(GL3);

            BeginMode = BeginMode.Quads;

            Create();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute("in_vertex", 3);

            AddTexture("ents");
            SetTexture("ents", TextureManager.Ents.TexArray);

            _scaleLoc = GL.GetUniformLocation(Program, "scale");
            _positionLoc = GL.GetUniformLocation(Program, "position");
            _sizeLoc = GL.GetUniformLocation(Program, "size");
            _textureLoc = GL.GetUniformLocation(Program, "texture");
        }

        public void Begin()
        {
            _sVB.Begin(this);
        }

        public new void End()
        {
            _sVB.End();
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            GL.Uniform2(_scaleLoc, 16.0f / Camera.Width * Camera.Scale, 16.0f / Camera.Height * Camera.Scale);

            GL.Enable(EnableCap.DepthTest);
        }

        public void Render(Vector3 pos, Vector2 size, ushort texIndex)
        {
            GL.Uniform3(_positionLoc, ref pos);
            GL.Uniform2(_sizeLoc, ref size);
            GL.Uniform1(_textureLoc, (float) texIndex);

            _sVB.Render();
        }

        protected override void OnEnd()
        {
            GL.Disable(EnableCap.DepthTest);
        }
    }
}
