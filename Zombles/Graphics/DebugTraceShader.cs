using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using OpenTKTK.Shaders;
using OpenTKTK.Utils;

using Zombles.Geometry;
using Zombles.Entities;

namespace Zombles.Graphics
{
    public class DebugTraceShader : WorldShader
    {
        private int _colourLoc;
        private Color4 _colour;

        public Color4 Colour
        {
            get { return _colour; }
            set
            {
                _colour = value;
                if (Started)
                    GL.End();
                GL.Uniform4(_colourLoc, _colour);
                if (Started)
                    GL.Begin(BeginMode);
            }
        }

        public DebugTraceShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, false);
            vert.AddUniform(ShaderVarType.Mat4, "vp_matrix");
            vert.AddUniform(ShaderVarType.Vec2, "world_offset");
            vert.AddAttribute(ShaderVarType.Vec2, "in_vertex");
            vert.Logic = @"
                void main( void )
                {
                    const float yscale = 2.0 / sqrt( 3.0 );

                    gl_Position = vp_matrix * vec4(
                        in_vertex.x + world_offset.x,
                        0.5 * yscale,
                        in_vertex.y + world_offset.y,
                        1.0
                    );
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, false);
            frag.AddUniform(ShaderVarType.Vec4, "colour");
            frag.FragOutIdentifier = "out_frag_colour";
            frag.Logic = @"
                void main( void )
                {
                    out_frag_colour = colour;
                }
            ";

            VertexSource = vert.Generate(GL3);
            FragmentSource = frag.Generate(GL3);

            BeginMode = BeginMode.Lines;

            Create();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute("in_vertex", 2);

            _colourLoc = GL.GetUniformLocation(Program, "colour");
            Colour = new Color4(255, 255, 255, 127);
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public void Render(TraceResult trace)
        {
            GL.VertexAttrib2(Attributes[0].Location, trace.Origin);
            GL.VertexAttrib2(Attributes[0].Location, trace.Origin + trace.Vector);
        }

        public void Render(PathEdge path)
        {
            GL.VertexAttrib2(Attributes[0].Location, path.Origin);
            GL.VertexAttrib2(Attributes[0].Location, path.Origin + path.Vector);
        }

        public void Render(float x0, float y0, float x1, float y1)
        {
            GL.VertexAttrib2(Attributes[0].Location, x0, y0);
            GL.VertexAttrib2(Attributes[0].Location, x1, y1);
        }

        public void Render(Path path)
        {
            GL.VertexAttrib2(Attributes[0].Location, path.Origin);
            Vector2 prev = path.Origin;
            for (int i = 0; i < path.Waypoints.Length; ++i) {
                GL.VertexAttrib2(Attributes[0].Location, prev + path.City.Difference(prev, path.Waypoints[i].Entity.Position2D));
                GL.VertexAttrib2(Attributes[0].Location, path.Waypoints[i].Entity.Position2D);
                prev = path.Waypoints[i].Entity.Position2D;
            }
            GL.VertexAttrib2(Attributes[0].Location, path.Desination);
        }

        protected override void OnEnd()
        {
            base.OnEnd();

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
        }
    }
}
