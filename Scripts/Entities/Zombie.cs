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
        public static int Count { get; private set; }

        private bool myCounted;

        private static EntityAnim stWalkAnim;
        private static EntityAnim stStandAnim;
        private static EntityAnim stDeadAnim;

        private float myMoveSpeed;

        private double myTurnTime;
        private double myNextBleed;
        private double myBleedTime;

        public override EntityAnim WalkAnim
        {
            get
            {
                if (stWalkAnim == null)
                    stWalkAnim = EntityAnim.GetAnim("anims", "zombie", "walk");

                return stWalkAnim;
            }
        }

        public override EntityAnim StandAnim
        {
            get
            {
                if (stStandAnim == null)
                    stStandAnim = EntityAnim.GetAnim("anims", "zombie", "stand");

                return stStandAnim;
            }
        }

        public override EntityAnim DeadAnim
        {
            get
            {
                if (stDeadAnim == null)
                    stDeadAnim = EntityAnim.GetAnim("anims", "zombie", "dead");

                return stDeadAnim;
            }
        }

        public override float MoveSpeed
        {
            get { return myMoveSpeed; }
        }

        public Zombie(Entity ent)
            : base(ent)
        {
            myMoveSpeed = Tools.Random.NextSingle() * 0.5f + 0.5f;

            if (Tools.Random.NextSingle() < 0.05)
                myMoveSpeed += 2.0f;

            myBleedTime = Tools.Random.NextDouble() * 3.0 + 2.0;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            if (Health.IsAlive && Health.MaxHealth == 1) {
                Health.SetMaximum(50);
                Health.Revive();
            }

            if (!myCounted) {
                ++Count;
                myCounted = true;
            }

            myTurnTime = ZomblesGame.Time;
            myNextBleed = ZomblesGame.Time + Tools.Random.NextDouble() * 0.125;
        }

        public override void OnThink(double dt)
        {
            base.OnThink(dt);

            if (ZomblesGame.Time - myTurnTime < myBleedTime && ZomblesGame.Time >= myNextBleed) {
                City.SplashBlood(Position2D, 0.5f);
                myNextBleed = ZomblesGame.Time + Tools.Random.NextDouble();
            }
        }

        public override void OnRemove()
        {
            base.OnRemove();

            if (myCounted) {
                --Count;
                myCounted = false;
            }
        }
    }
}
