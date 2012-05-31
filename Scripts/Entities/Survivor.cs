using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zombles.Entities;
using Zombles.Graphics;

namespace Zombles.Scripts.Entities
{
    public class Survivor : Human
    {
        public static int Count { get; private set; }

        private bool myCounted;

        private static EntityAnim stWalkAnim;
        private static EntityAnim stStandAnim;

        public float TiredSpeed { get; private set; }
        public float WalkSpeed { get; private set; }
        public float RunSpeed { get; private set; }

        public float MaxStamina { get; private set; }

        public float RecoverRate { get; private set; }

        public float Stamina
        {
            get; private set;
        }

        public bool Running
        {
            get;
            private set;
        }

        public bool CanRun
        {
            get { return !Running && Stamina > Math.Max( 0.5f, MaxStamina / 2.0 ); }
        }

        public override EntityAnim WalkAnim
        {
            get
            {
                if ( stWalkAnim == null )
                    stWalkAnim = EntityAnim.GetAnim( "human walk" );

                return stWalkAnim;
            }
        }

        public override EntityAnim StandAnim
        {
            get
            {
                if ( stStandAnim == null )
                    stStandAnim = EntityAnim.GetAnim( "human stand" );

                return stStandAnim;
            }
        }

        public override float MoveSpeed
        {
            get
            {
                return Running ? RunSpeed :
                    Stamina < MaxStamina ? TiredSpeed :
                    WalkSpeed;
            }
        }

        public Survivor( Entity ent )
            : base( ent )
        {
            float speedMul = Tools.Random.NextSingle() * 0.5f + 0.75f;

            TiredSpeed = 0.75f * speedMul;
            WalkSpeed  = 1.00f * speedMul;
            RunSpeed   = 3.00f * speedMul;

            MaxStamina = 3.0f + Tools.Random.NextSingle() * 4.0f;
            Stamina = MaxStamina;

            RecoverRate = 1.0f / 3.0f;
        }

        public void Zombify()
        {
            City.SplashBlood( Position2D, 2.0f );
            StopMoving();

            Entity.SwapComponent<Survivor, Zombie>();
            Entity.SwapComponent<SurvivorAI, ZombieAI>();

            Entity.UpdateComponents();
        }

        public override void OnThink( double dt )
        {
            if ( Running )
            {
                Stamina = Math.Max( 0, Stamina - (float) dt );

                if ( Stamina == 0.0f )
                    StopRunning();
            }
            else
            {
                if ( Stamina < MaxStamina )
                {
                    Stamina += (float) ( dt * RecoverRate );

                    if ( Stamina >= MaxStamina )
                    {
                        Stamina = MaxStamina;
                        UpdateSpeed();
                    }
                }
            }
        }

        public void StartRunning()
        {
            if ( CanRun )
            {
                Stamina -= 0.5f;
                Running = true;
                UpdateSpeed();
            }
        }

        public void StopRunning()
        {
            if ( Running )
            {
                Running = false;
                UpdateSpeed();
            }
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            if ( !myCounted )
            {
                ++Count;
                myCounted = true;
            }
        }

        public override void OnRemove()
        {
            base.OnRemove();

            if ( myCounted )
            {
                --Count;
                myCounted = false;
            }
        }
    }
}
