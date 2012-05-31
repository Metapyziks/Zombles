using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Entities;
using Zombles.Graphics;

namespace Zombles.Scripts.Entities
{
    public abstract class Human : Component
    {
        protected Movement Movement { get; private set; }
        protected RenderAnim Anim { get; private set; }

        public abstract EntityAnim WalkAnim { get; }
        public abstract EntityAnim StandAnim { get; }

        public float MoveSpeed { get; set; }
        public float ViewRadius { get; set; }

        public Human( Entity ent )
            : base( ent )
        {
            MoveSpeed = 1.0f;
            ViewRadius = 16.0f;
        }

        public override void OnSpawn()
        {
            if ( Entity.HasComponent<Movement>() )
                Movement = Entity.GetComponent<Movement>();

            if ( Entity.HasComponent<RenderAnim>() )
                Anim = Entity.GetComponent<RenderAnim>();

            Anim.Start( StandAnim );
        }

        public void Turn( Vector2 dir )
        {
            Anim.Rotation = (float) Math.Atan2( dir.Y, dir.X );
        }

        public void StartMoving( Vector2 dir )
        {
            dir.Normalize();
            Movement.Velocity = dir * MoveSpeed;

            Anim.Start( WalkAnim );
            Anim.Speed = MoveSpeed;

            Turn( dir );
        }

        public void StopMoving()
        {
            Movement.Stop();

            Anim.Start( StandAnim );
        }
    }
}
