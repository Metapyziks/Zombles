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
        private double myStartTime;

        public EntityAnim CurAnim { get; private set; }
        public EntityAnim NextAnim { get; private set; }

        public bool Playing { get; private set; }

        public float Rotation { get; set; }
        public double Speed { get; set; }

        public RenderAnim( Entity ent )
            : base( ent )
        {
            myStartTime = 0.0;

            CurAnim = null;
            NextAnim = null;
            Rotation = 0.0f;
            Speed = 1.0;
        }

        public void Start( EntityAnim anim )
        {
            Start( anim, anim );
        }

        public void Start( EntityAnim anim, EntityAnim nextAnim )
        {
            myStartTime = ZomblesGame.Time;
            CurAnim = anim;
            NextAnim = nextAnim;
            Playing = true;
        }

        public void Stop()
        {
            Playing = false;
        }

        public override void OnRender( FlatEntityShader shader )
        {
            if ( CurAnim == null )
                return;

            Size = CurAnim.Size;

            int frame = 0;
            if ( Playing )
            {
                frame = (int) ( ( ZomblesGame.Time - myStartTime ) *
                    Speed * CurAnim.Frequency * CurAnim.FrameCount );

                if ( frame >= CurAnim.FrameCount )
                {
                    myStartTime += 1.0 / ( CurAnim.Frequency * Speed );
                    CurAnim = NextAnim;

                    if ( CurAnim == null )
                    {
                        Playing = false;
                        return;
                    }
                    
                    frame = (int) ( ( ZomblesGame.Time - myStartTime ) *
                        Speed * CurAnim.Frequency * CurAnim.FrameCount ) % CurAnim.FrameCount;
                }
            }

            if ( CurAnim.IsDirectional )
            {
                float diff = Tools.AngleDif( shader.Camera.Rotation.Y, Rotation );
                int dir = (int) Math.Round( diff * 2.0f / MathHelper.Pi + 2 ) % 4;
                TextureIndex = CurAnim.FrameIndices[ dir, frame ];
            }
            else
                TextureIndex = CurAnim.FrameIndices[ 0, frame ];

            base.OnRender( shader );
        }
    }
}
