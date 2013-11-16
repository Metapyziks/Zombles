﻿using OpenTK;
using OpenTK.Graphics.OpenGL;

using OpenTKTK.Utils;

namespace Zombles.Graphics
{
    public class ModelEntityShader : WorldShader
    {
        private int _transformLoc;

        public ModelEntityShader()
        {
            var vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "vp_matrix");
            vert.AddUniform(ShaderVarType.Mat4, "mdl_matrix");
            vert.AddUniform(ShaderVarType.Vec2, "world_offset");
            vert.AddAttribute(ShaderVarType.Vec3, "in_position");
            vert.AddAttribute(ShaderVarType.Vec3, "in_texture");
            vert.AddAttribute(ShaderVarType.Vec3, "in_normal");
            vert.AddVarying(ShaderVarType.Float, "var_shade");
            vert.AddVarying(ShaderVarType.Vec3, "var_texture");
            vert.Logic = @"
                void main(void)
                {
                    var_texture = in_texture;

                    const vec3 sun_dir = normalize(vec3(1.0, -2.0, 0.0));

                    var_shade = 0.75 + abs(dot(sun_dir, (mdl_matrix * vec4(in_normal, 0.0)).xyz)) * 0.25;

                    vec3 world_pos = mdl_matrix * vec4(
                        in_position.x,
                        in_position.y,
                        in_position.z,
                        1.0
                    );

                    gl_Position = vp_matrix * vec4(
                        world_pos.x + world_offset.x,
                        world_pos.y,
                        world_pos.z + world_offset.y,
                        1.0
                    );
                }
            ";

            var frag = new ShaderBuilder(ShaderType.FragmentShader, false, vert);
            frag.AddUniform(ShaderVarType.Sampler2DArray, "ents");
            frag.FragOutIdentifier = "out_frag_colour";
            frag.Logic = @"
                void main(void)
                {
                    vec4 clr = texture2DArray(ents, var_texture);
                    if(clr.a < 0.5) discard;
                    out_frag_colour = vec4(clr.rgb * var_shade, 1.0);
                }
            ";

            VertexSource = vert.Generate();
            FragmentSource = frag.Generate();

            BeginMode = BeginMode.Quads;

            Create();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute("in_position", 3);
            AddAttribute("in_texture", 3);
            AddAttribute("in_normal", 3);

            AddTexture("ents");

            _transformLoc = GL.GetUniformLocation(Program, "mdl_matrix");
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

        public void Render(EntityModel model, int skin, Matrix4 transform)
        {
            GL.UniformMatrix4(_transformLoc, false, ref transform);

            model.Render(skin);
        }

        protected override void OnEnd()
        {
            GL.Disable(EnableCap.DepthTest);
        }
    }
}
