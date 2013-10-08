using System;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using OpenTKTK.Shaders;
using OpenTKTK.Utils;

namespace Zombles.Graphics
{
    public class GeometryShader : WorldShader
    {
        public GeometryShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "vp_matrix");
            vert.AddUniform(ShaderVarType.Vec2, "world_offset");
            vert.AddUniform(ShaderVarType.Vec2, "world_size");
            vert.AddAttribute(ShaderVarType.Vec3, "in_vertex");
            vert.AddVarying(ShaderVarType.Vec3, "var_tex");
            vert.AddVarying(ShaderVarType.Float, "var_shade");
            vert.AddVarying(ShaderVarType.Vec2, "var_blood_tex");
            vert.AddVarying(ShaderVarType.Float, "var_blood");
            vert.Logic = @"
                void main(void)
                {
                    int dat = int(in_vertex.z);

                    int ix = int(in_vertex.x);
                    int iz = int(in_vertex.y);

                    int x = ix & 0xfff;
                    int z = iz & 0xfff;

                    var_tex = vec3(
                        float(dat & 0x1),
                        float((dat >> 1) & 0x3) / 2.0,
                        float((dat >> 8) & 0xffff)
                    );

                    const float yscale = 1.0 / sqrt(3.0);
                    const vec2 normals[] = vec2[4]
                    (
                        vec2( 0.5,  0.0),
                        vec2( 0.0,  0.5),
                        vec2(-0.5,  0.0),
                        vec2( 0.0, -0.5)
                    );

                    float y = float((dat >> 4) & 0xf);
                    vec2 bloodadd;

                    if (y > 0.0f) {
                        int normalno = ((ix >> 12) & 0x1) | ((iz >> 11) & 0x2);
                        bloodadd = normals[normalno];
                    } else {
                        bloodadd = vec2(0.0, 0.0);
                    }

                    var_shade = 1.0 - 0.125 * float((dat >> 3) & 0x1);
                    var_blood = max(1.0 - y / 2.0, 0.0);

                    var_blood_tex = vec2(
                        (z + bloodadd.y) / world_size.y,
                        (x + bloodadd.x) / world_size.x
                    );

                    gl_Position = vp_matrix * vec4(
                        x + world_offset.x,
                        y * yscale,
                        z + world_offset.y,
                        1.0
                    );
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false, vert);
            frag.AddUniform(ShaderVarType.Sampler2DArray, "tiles");
            frag.AddUniform(ShaderVarType.Sampler2D, "bloodmap");
            frag.FragOutIdentifier = "out_frag_colour";
            frag.Logic = @"
                void main(void)
                {
                    vec4 clr = texture2DArray(tiles, var_tex);
                    if (clr.a < 1.0)
                        discard;

                    if (var_blood > 0.0 && texture2D(bloodmap, var_blood_tex).a * var_blood >= 0.25)
                        clr = clr * 0.5 + vec4(0.3, 0.0, 0.0, 0.0);
   
                    out_frag_colour = vec4(clr.rgb * var_shade, 1.0);
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

            AddTexture("tiles");
            AddTexture("bloodmap");
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            SetTexture("tiles", TextureManager.Tiles.TexArray);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            GL.CullFace(CullFaceMode.Front);
        }

        protected override void OnEnd()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
        }
    }
}
