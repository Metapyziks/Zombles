using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTKTK.Shaders;
using OpenTKTK.Utils;

namespace Zombles.Graphics
{
    public class WorldShader : ShaderProgram3D<OrthoCamera>
    {
        private int _worldOffsetLoc;
        private int _worldSizeLoc;

        public bool IsTopDown
        {
            get { return Camera.Pitch >= Math.PI * 0.49; }
        }

        protected override void ConstructVertexShader(OpenTKTK.Utils.ShaderBuilder vert)
        {
            base.ConstructVertexShader(vert);

            vert.AddUniform(ShaderVarType.Vec2, "world_offset");
            vert.AddUniform(ShaderVarType.Vec2, "world_size");
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            _worldOffsetLoc = GetUniformLocation("world_offset");
            _worldSizeLoc = GetUniformLocation("world_size");
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            var offset = Camera.WorldOffset;

            GL.Uniform2(_worldOffsetLoc, ref offset);

            if (_worldSizeLoc != -1) {
                GL.Uniform2(_worldSizeLoc, (float) Camera.WrapWidth, (float) Camera.WrapHeight);
            }
        }
    }
}
