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
        public Movement Movement { get; private set; }
        public RenderAnim Anim { get; private set; }
        public Health Health { get; private set; }

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

            if ( Entity.HasComponent<Health>() )
            {
                Health = Entity.GetComponent<Health>();

                Health.Healed += OnHealed;
                Health.Damaged += OnDamaged;
                Health.Killed += OnKilled;
            }

            Anim.Start( StandAnim );
        }

        public override void OnRemove()
        {
            if ( Health != null )
            {
                Health.Healed -= OnHealed;
                Health.Damaged -= OnDamaged;
                Health.Killed -= OnKilled;
            }
        }

        protected virtual void OnHealed( object sender, HealedEventArgs e )
        {
            UpdateSpeed();
        }

        protected virtual void OnDamaged( object sender, DamagedEventArgs e )
        {
            City.SplashBlood( Position2D, Math.Min( 0.25f * e.Damage + 0.5f, 4.0f ) );
            UpdateSpeed();
        }

        protected virtual void OnKilled( object sender, KilledEventArgs e )
        {
            City.SplashBlood( Position2D, 4.0f );
            StopMoving();
        }

        public void Attack( Vector2 dir )
        {

        }

        public void FaceDirection( Vector2 dir )
        {
            if ( !Health.IsAlive )
                return;

            Anim.Rotation = (float) Math.Atan2( dir.Y, dir.X );
        }

        public void StartMoving( Vector2 dir )
        {
            if ( !Health.IsAlive )
                return;

            if ( !Anim.Playing || !Movement.IsMoving || Anim.CurAnim != WalkAnim )
                Anim.Start( WalkAnim );

            dir.Normalize();
            Movement.Velocity = dir * MoveSpeed;

            Anim.Speed = MoveSpeed;

            FaceDirection( dir );
        }

        public void UpdateSpeed()
        {
            if ( !Movement.IsMoving )
                return;

            StartMoving( Movement.Velocity );
        }

        public void StopMoving()
        {
            if ( !Anim.Playing || Movement.IsMoving || Anim.CurAnim != StandAnim )
                Anim.Start( StandAnim );

            Movement.Stop();
        }
    }
}
