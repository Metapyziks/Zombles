using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTKTK.Shaders;

namespace Zombles.Graphics
{
    public class WorldShader : ShaderProgram3D<OrthoCamera>
    {
        private int _worldOffsetLoc;
        private int _worldSizeLoc;

        protected override void OnCreate()
        {
            base.OnCreate();

            _worldOffsetLoc = GL.GetUniformLocation(Program, "world_offset");
            _worldSizeLoc = GL.GetUniformLocation(Program, "world_size");
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            GL.Uniform2(_worldOffsetLoc, Camera.WorldOffset);

            if (_worldSizeLoc != -1) {
                GL.Uniform2(_worldSizeLoc, Camera.WrapWidth, Camera.WrapHeight);
            }
        }
    }
}
