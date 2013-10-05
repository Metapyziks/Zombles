using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class ShaderProgram3D : ShaderProgram
    {
        private int _viewMatrixLoc;
        private int _worldOffsetLoc;
        private int _worldSizeLoc;

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public OrthoCamera Camera
        {
            get;
            set;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            _viewMatrixLoc = GL.GetUniformLocation(Program, "view_matrix");
            _worldOffsetLoc = GL.GetUniformLocation(Program, "world_offset");
            _worldSizeLoc = GL.GetUniformLocation(Program, "world_size");
        }

        protected override void OnStartBatch()
        {
            if (Camera != null) {
                Matrix4 viewMat = Camera.ViewMatrix;
                Vector2 worldOffset = Camera.WorldOffset;
                GL.UniformMatrix4(_viewMatrixLoc, false, ref viewMat);
                GL.Uniform2(_worldOffsetLoc, ref worldOffset);

                if (_worldSizeLoc != -1)
                    GL.Uniform2(_worldSizeLoc, (float) Camera.WrapWidth, (float) Camera.WrapHeight);
            }
        }
    }
}
