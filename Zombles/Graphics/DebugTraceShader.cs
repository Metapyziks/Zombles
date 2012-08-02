using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Zombles.Geometry;
using Zombles.Entities;

namespace Zombles.Graphics
{
    public class DebugTraceShader : ShaderProgram3D
    {
        private int myColourLoc;
        private Color4 myColour;

        public Color4 Colour
        {
            get { return myColour; }
            set
            {
                myColour = value;
                if ( Started )
                    GL.End();
                GL.Uniform4( myColourLoc, myColour );
                if ( Started )
                    GL.Begin( BeginMode );
            }
        }

        public DebugTraceShader()
        {
            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, false );
            vert.AddUniform( ShaderVarType.Mat4, "view_matrix" );
            vert.AddUniform( ShaderVarType.Vec2, "world_offset" );
            vert.AddAttribute( ShaderVarType.Vec2, "in_vertex" );
            vert.Logic = @"
                void main( void )
                {
                    const float yscale = 2.0 / sqrt( 3.0 );

                    gl_Position = view_matrix * vec4(
                        in_vertex.x + world_offset.x,
                        0.5 * yscale,
                        in_vertex.y + world_offset.y,
                        1.0
                    );
                }
            ";

            ShaderBuilder frag = new ShaderBuilder( ShaderType.FragmentShader, false );
            frag.AddUniform( ShaderVarType.Vec4, "colour" );
            frag.Logic = @"
                void main( void )
                {
                    out_frag_colour = colour;
                }
            ";

            VertexSource = vert.Generate( GL3 );
            FragmentSource = frag.Generate( GL3 );

            BeginMode = BeginMode.Lines;

            Create();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute( "in_vertex", 2 );

            myColourLoc = GL.GetUniformLocation( Program, "colour" );
            Colour = Color4.White;
        }

        protected override void OnStartBatch()
        {
            base.OnStartBatch();

            GL.Enable( EnableCap.DepthTest );
            GL.Enable( EnableCap.Blend );

            GL.BlendFunc( BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha );
        }

        public void Render( TraceResult trace )
        {
            GL.VertexAttrib2( Attributes[ 0 ].Location, trace.Origin );
            GL.VertexAttrib2( Attributes[ 0 ].Location, trace.Origin + trace.Vector );
        }

        public void Render( PathEdge path )
        {
            GL.VertexAttrib2( Attributes[ 0 ].Location, path.Origin );
            GL.VertexAttrib2( Attributes[ 0 ].Location, path.Origin + path.Vector );
        }

        protected override void OnEndBatch()
        {
            base.OnEndBatch();

            GL.Disable( EnableCap.DepthTest );
            GL.Disable( EnableCap.Blend );
        }
    }
}
