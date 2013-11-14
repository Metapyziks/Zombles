using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTKTK.Utils;
using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class MeshEntityShader : WorldShader
    {
        private int _transformLoc;

        public MeshEntityShader()
        {
            var vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "vp_matrix");
            vert.AddUniform(ShaderVarType.Mat4, "transform");
            vert.AddAttribute(ShaderVarType.Vec3, "in_position");
            vert.AddAttribute(ShaderVarType.Vec3, "in_texture");
            vert.AddVarying(ShaderVarType.Vec3, "var_texture");
            vert.Logic = @"
                void main(void)
                {
                    var_texture = in_texture;
                    gl_Position = vp_matrix * (transform * in_position);
                }
            ";

            var frag = new ShaderBuilder(ShaderType.FragmentShader, false, vert);
            frag.AddUniform(ShaderVarType.Sampler2DArray, "ents");
            vert.FragOutIdentifier = "out_frag_colour";
            frag.Logic = @"
                void main(void)
                {
                    out_frag_colour = texture2DArray(ents, var_texture);
                    if(out_frag_colour.a < 0.5) discard;
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
            AddAttribute("in_texture", 3);

            AddTexture("ents");

            _transformLoc = GL.GetUniformLocation(Program, "transform");
        }

        public void Begin()
        {
            EntityModel.VertexBuffer.Begin(this);
        }

        public new void End()
        {
            EntityModel.VertexBuffer.End();
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            SetTexture("ents", TextureManager.Ents.TexArray);

            GL.Enable(EnableCap.DepthTest);
        }

        public void Render(EntityModel model, Matrix4 transform)
        {
            GL.UniformMatrix4(_transformLoc, false, ref transform);

            model.Render();
        }

        protected override void OnEnd()
        {
            GL.Disable(EnableCap.DepthTest);
        }
    }
}
