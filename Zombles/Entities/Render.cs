using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zombles.Graphics;

namespace Zombles.Entities
{
    public abstract class Render2D : Component
    {
        public Render2D( Entity ent )
            : base( ent )
        {

        }

        /*
        public abstract void OnRender( EntShader2D shader )
        {

        }
        */
    }

    public abstract class Render3D : Component
    {
        public Render3D( Entity ent )
            : base( ent )
        {

        }

        /*
        public abstract void OnRender( EntShader3D shader )
        {

        }
        */
    }
}
