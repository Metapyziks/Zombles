using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Entities;
using Zombles.Graphics;

namespace Zombles.Scripts.Entities
{
    public class Zombie : Human
    {
        private static EntityAnim _sWalkAnim;
        private static EntityAnim _sStandAnim;
        private static EntityAnim _sDeadAnim;

        private float _moveSpeed;

        private double _turnTime;
        private double _nextBleed;
        private double _bleedTime;

        public override EntityAnim WalkAnim
        {
            get
            {
                if (_sWalkAnim == null)
                    _sWalkAnim = EntityAnim.GetAnim("anims", "zombie", "walk");

                return _sWalkAnim;
            }
        }

        public override EntityAnim StandAnim
        {
            get
            {
                if (_sStandAnim == null)
                    _sStandAnim = EntityAnim.GetAnim("anims", "zombie", "stand");

                return _sStandAnim;
            }
        }

        public override EntityAnim DeadAnim
        {
            get
            {
                if (_sDeadAnim == null)
                    _sDeadAnim = EntityAnim.GetAnim("anims", "zombie", "dead");

                return _sDeadAnim;
            }
        }

        public override float MoveSpeed
        {
            get { return _moveSpeed; }
        }

        public Zombie(Entity ent)
            : base(ent)
        {
            _moveSpeed = Tools.Random.NextSingle() * 0.5f + 0.5f;

            if (Tools.Random.NextSingle() < 0.05)
                _moveSpeed += 2.0f;

            _bleedTime = Tools.Random.NextDouble() * 15.0 + 5.0;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            if (Health.IsAlive && Health.MaxHealth == 1) {
                Health.SetMaximum(50);
                Health.Revive();
            }

            _turnTime = ZomblesGame.Time;
            _nextBleed = ZomblesGame.Time + Tools.Random.NextDouble() * 0.125;
        }

        public override void OnThink(double dt)
        {
            base.OnThink(dt);

            if (ZomblesGame.Time - _turnTime < _bleedTime && ZomblesGame.Time >= _nextBleed) {
                City.SplashBlood(Position2D, 0.5f);
                _nextBleed = ZomblesGame.Time + Tools.Random.NextDouble();
            }
        }
    }
}
