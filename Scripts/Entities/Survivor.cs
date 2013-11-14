using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Zombles.Entities;
using Zombles.Graphics;

namespace Zombles.Scripts.Entities
{
    public class Survivor : Human
    {
        private static EntityAnim _sWalkAnim;
        private static EntityAnim _sStandAnim;
        private static EntityAnim _sDeadAnim;

        public bool IsInfected { get; private set; }

        public float TiredSpeed { get; private set; }
        public float WalkSpeed { get; private set; }
        public float RunSpeed { get; private set; }

        public float MaxStamina { get; private set; }

        public float RecoverRate { get; private set; }

        public float Stamina
        {
            get;
            private set;
        }

        public bool IsRunning
        {
            get;
            private set;
        }

        public bool CanRun
        {
            get { return !IsRunning && Stamina > Math.Max(0.5f, MaxStamina / 2.0); }
        }

        public override EntityAnim WalkAnim
        {
            get
            {
                if (_sWalkAnim == null)
                    _sWalkAnim = EntityAnim.Get("anims", "human", "walk");

                return _sWalkAnim;
            }
        }

        public override EntityAnim StandAnim
        {
            get
            {
                if (_sStandAnim == null)
                    _sStandAnim = EntityAnim.Get("anims", "human", "stand");

                return _sStandAnim;
            }
        }

        public override EntityAnim DeadAnim
        {
            get
            {
                if (_sDeadAnim == null)
                    _sDeadAnim = EntityAnim.Get("anims", "human", "dead");

                return _sDeadAnim;
            }
        }

        public override float MoveSpeed
        {
            get
            {
                return !Health.IsAlive ? 0.0f :
                    (IsRunning ? RunSpeed : Stamina < MaxStamina ? TiredSpeed : WalkSpeed) *
                    (Health.Value < 60 ? Math.Max(Health.Value / 60.0f, 0.125f) : 1.0f);
            }
        }

        public Survivor(Entity ent)
            : base(ent)
        {
            float speedMul = Tools.Random.NextSingle() * 0.5f + 1.5f;

            TiredSpeed = 0.75f * speedMul;
            WalkSpeed = 1.00f * speedMul;
            RunSpeed = 2.00f * speedMul;

            MaxStamina = 3.0f + Tools.Random.NextSingle() * 4.0f;
            Stamina = MaxStamina;

            RecoverRate = 1.0f / 3.0f;

            IsInfected = false;
        }

        public void Infect()
        {
            IsInfected = true;
        }

        public void Zombify()
        {
            StopMoving();
            World.SplashBlood(Position2D, 4.0f);

            Entity.SwapComponent<Human, Zombie>();

            if (Entity.HasComponent<HumanControl>()) {
                Entity.SwapComponent<HumanControl, ZombieAI>();
            } else {
                Entity.AddComponent<ZombieAI>();
            }

            Entity.UpdateComponents();

            Health.Revive();
        }

        protected override void OnDamaged(object sender, DamagedEventArgs e)
        {
            base.OnDamaged(sender, e);

            if (!IsInfected && e.HasAttacker && e.Attacker.HasComponent<Zombie>() && Tools.Random.NextDouble() < 0.37)
                Infect();
        }

        protected override void OnKilled(object sender, KilledEventArgs e)
        {
            if (IsInfected && Tools.Random.NextDouble() < 0.74)
                Zombify();
            else
                base.OnKilled(sender, e);
        }

        public override void OnThink(double dt)
        {
            if (IsRunning) {
                Stamina = Math.Max(0, Stamina - (float) dt);

                if (Stamina == 0.0f)
                    StopRunning();
            } else {
                if (Stamina < MaxStamina) {
                    Stamina += (float) (dt * RecoverRate);

                    if (Stamina >= MaxStamina) {
                        Stamina = MaxStamina;
                        UpdateSpeed();
                    }
                }
            }
        }

        public void StartRunning()
        {
            if (CanRun) {
                Stamina -= 0.5f;
                IsRunning = true;
                UpdateSpeed();
            }
        }

        public void StopRunning()
        {
            if (IsRunning) {
                IsRunning = false;
                UpdateSpeed();
            }
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            if (Health.IsAlive && Health.MaxHealth == 1) {
                Health.SetMaximum(100);
                Health.Revive();
            }
        }
    }
}
