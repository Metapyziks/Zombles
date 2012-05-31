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
        private int myViewMatrixLoc;
        private int myWorldOffsetLoc;
        private int myWorldSizeLoc;

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public Camera Camera
        {
            get;
            set;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            myViewMatrixLoc = GL.GetUniformLocation( Program, "view_matrix" );
            myWorldOffsetLoc = GL.GetUniformLocation( Program, "world_offset" );
            myWorldSizeLoc = GL.GetUniformLocation( Program, "world_size" );
        }

        protected override void OnStartBatch()
        {
            if ( Camera != null )
            {
                Matrix4 viewMat = Camera.ViewMatrix;
                Vector2 worldOffset = Camera.WorldOffset;
                GL.UniformMatrix4( myViewMatrixLoc, false, ref viewMat );
                GL.Uniform2( myWorldOffsetLoc, ref worldOffset );

                if ( myWorldSizeLoc != -1 )
                    GL.Uniform2( myWorldSizeLoc, (float) Camera.WrapWidth, (float) Camera.WrapHeight );
            }
        }
    }
}
