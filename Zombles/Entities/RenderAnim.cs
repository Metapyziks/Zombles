using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Graphics;

namespace Zombles.Entities
{
    public class RenderAnim : Render
    {
        public EntityAnim Anim { get; set; }
        public float Rotation { get; set; }

        public RenderAnim( Entity ent )
            : base( ent )
        {
            Anim = null;
            Rotation = 0.0f;
        }

        public override void OnRender( FlatEntityShader shader )
        {
            if ( Anim != null )
            {
                if ( Anim.IsDirectional )
                {
                    float diff = Tools.AngleDif( shader.Camera.Rotation.Y, Rotation );
                    int dir = (int) Math.Round( diff * 2.0f / MathHelper.Pi + 2 ) % 4;
                    TextureIndex = Anim.FrameIndices[ dir, 0 ];
                }
                else
                    TextureIndex = Anim.FrameIndices[ 0, 0 ];

                base.OnRender( shader );
            }
        }
    }
}
