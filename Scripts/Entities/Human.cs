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

        public abstract float MoveSpeed { get; }

        public Human( Entity ent )
            : base( ent )
        {

        }

        public override void OnSpawn()
        {
            if ( Entity.HasComponent<Movement>() )
                Movement = Entity.GetComponent<Movement>();

            if ( Entity.HasComponent<RenderAnim>() )
                Anim = Entity.GetComponent<RenderAnim>();

            Anim.Start( StandAnim );
        }

        public void FaceDirection( Vector2 dir )
        {
            Anim.Rotation = (float) Math.Atan2( dir.Y, dir.X );
        }

        public void StartMoving( Vector2 dir )
        {
            if ( !Anim.Playing || !Movement.Moving )
                Anim.Start( WalkAnim );

            dir.Normalize();
            Movement.Velocity = dir * MoveSpeed;

            Anim.Speed = MoveSpeed;

            FaceDirection( dir );
        }

        public void UpdateSpeed()
        {
            if ( !Movement.Moving )
                return;

            StartMoving( Movement.Velocity );
        }

        public void StopMoving()
        {
            if ( !Anim.Playing || Movement.Moving )
                Anim.Start( StandAnim );

            Movement.Stop();
        }
    }
}
