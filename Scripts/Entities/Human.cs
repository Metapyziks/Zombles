using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Entities;
using Zombles.Graphics;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    public abstract class Human : Component
    {
        private double _nextAttack;

        public Movement Movement { get; private set; }
        public RenderAnim Anim { get; private set; }
        public Health Health { get; private set; }

        public double AttackPeriod { get; protected set; }
        public int MinAttackDamage { get; protected set; }
        public int MaxAttackDamage { get; protected set; }

        public abstract EntityAnim WalkAnim { get; }
        public abstract EntityAnim StandAnim { get; }
        public abstract EntityAnim DeadAnim { get; }

        public abstract float BaseMoveSpeed { get; }

        public float MoveSpeed
        {
            get { return BaseMoveSpeed * (IsHoldingItem ? 0.5f : 1f); }
        }

        public virtual bool CanAttack
        {
            get { return MainWindow.Time >= _nextAttack; }
        }

        public Entity HeldItem
        {
            get { return Entity.Children.First(); }
        }

        public bool IsHoldingItem
        {
            get { return Entity.Children.Count() != 0; }
        }

        public Human(Entity ent)
            : base(ent)
        {
            _nextAttack = MainWindow.Time;

            AttackPeriod = 1.0;
            MinAttackDamage = 10;
            MaxAttackDamage = 25;
        }

        public override void OnSpawn()
        {
            Movement = null;
            Anim = null;
            Health = null;

            if (Entity.HasComponent<Movement>())
                Movement = Entity.GetComponent<Movement>();

            if (Entity.HasComponent<RenderAnim>())
                Anim = Entity.GetComponent<RenderAnim>();

            if (Entity.HasComponent<Health>()) {
                Health = Entity.GetComponent<Health>();

                Health.Healed += OnHealed;
                Health.Damaged += OnDamaged;
                Health.Killed += OnKilled;
            }

            if (!Anim.Playing)
                Anim.Start(StandAnim);
        }

        public override void OnRemove()
        {
            if (Health != null) {
                Health.Healed -= OnHealed;
                Health.Damaged -= OnDamaged;
                Health.Killed -= OnKilled;
            }
        }

        public void DropItem()
        {
            if (!IsHoldingItem) return;
            var item = HeldItem;
            Entity.RemoveChild(item);
            item.GetComponent<Item>().OnDrop(Entity);
        }

        public void PickupItem(Entity item)
        {
            var comp = item.GetComponentOrNull<Item>();
            if (comp == null || !comp.CanPickup(Entity)) return;

            if (comp.OnPickup(Entity)) Entity.AddChild(item);
        }

        protected virtual void OnHealed(object sender, HealedEventArgs e)
        {
            UpdateSpeed();
        }

        protected virtual void OnDamaged(object sender, DamagedEventArgs e)
        {
            World.SplashBlood(Position2D, Math.Min(0.25f * e.Damage + 0.5f, 4.0f));
            UpdateSpeed();
        }

        protected virtual void OnKilled(object sender, KilledEventArgs e)
        {
            World.SplashBlood(Position2D, 4.0f);
            StopMoving();

            Anim.Start(DeadAnim);

            if (IsHoldingItem) DropItem();

            if (Entity.HasComponent<RouteNavigation>()) {
                Entity.RemoveComponent<RouteNavigation>();
            }
            
            Entity.RemoveComponent<HumanControl>();
            Entity.RemoveComponent<Collision>();
            Entity.RemoveComponent<Movement>();

            Entity.UpdateComponents();

            for (int i = 0; i < 4; ++i) {
                Face face = (Face) (1 << i);
                Vector2 ray = face.GetNormal() / 2.0f;
                TraceResult res = TraceLine.Quick(World, Position2D, Position2D + ray);
                if (res.Hit)
                    Entity.Position2D -= res.Vector;
            }
        }

        public virtual void Attack(Vector2 dir)
        {
            if (!Health.IsAlive || !CanAttack)
                return;

            _nextAttack = MainWindow.Time + AttackPeriod;

            FaceDirection(dir);

            TraceLine trace = new TraceLine(World);
            trace.HitGeometry = true;
            trace.HitEntities = true;
            trace.HitEntityPredicate = (x => x != Entity);
            trace.Origin = Position2D;
            trace.Normal = dir;
            trace.Length = 1.0f;

            TraceResult res = trace.GetResult();
            if (!res.HitEntity || !res.Entity.HasComponent<Health>()) return;

            res.Entity.GetComponent<Health>().Damage(Tools.Random.Next(MinAttackDamage, MaxAttackDamage + 1), Entity);
        }

        public void FaceDirection(Vector2 dir)
        {
            if (!Health.IsAlive)
                return;

            Anim.Rotation = (float) Math.Atan2(dir.Y, dir.X);
        }

        public void StartMoving(Vector2 dir)
        {
            if (Movement == null || !Health.IsAlive)
                return;

            if (!Anim.Playing || !Movement.IsMoving || Anim.CurAnim != WalkAnim)
                Anim.Start(WalkAnim);

            dir.Normalize();
            Movement.Velocity = dir * MoveSpeed;

            Anim.Speed = MoveSpeed;

            FaceDirection(dir);
        }

        public void UpdateSpeed()
        {
            if (Movement == null)
                return;

            if (!Movement.IsMoving)
                return;

            StartMoving(Movement.Velocity);
        }

        public void StopMoving()
        {
            if (Movement == null)
                return;

            if (!Anim.Playing || Movement.IsMoving || Anim.CurAnim != StandAnim)
                Anim.Start(StandAnim);

            Movement.Stop();
        }
    }
}
