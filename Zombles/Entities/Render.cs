using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Graphics;

namespace Zombles.Entities
{
    public class Render : Component
    {
        public Vector2 Size;

        public Render( Entity ent )
            : base( ent )
        {
            Size = new Vector2( 1.0f, 1.0f );
        }

        public virtual void OnRender( FlatEntityShader shader )
        {
            shader.Render( Entity.Position, Size );
        }
    }
}
