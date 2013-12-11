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
        private Vector2 _moveDir;

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
            get { return Entity.Children.Where(x => x.HasComponent<Item>()).First(); }
        }

        public bool IsHoldingItem
        {
            get { return Entity.Children.Count(x => x.HasComponent<Item>()) != 0; }
        }

        public bool IsSelected
        {
            get { return Entity.Children.Any(x => x.ClassName == "selection marker"); }
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

        public Entity DropItem()
        {
            if (!IsHoldingItem) return null;
            var item = HeldItem;
            Entity.RemoveChild(item);
            item.GetComponent<Item>().OnDrop(Entity);

            return item;
        }

        public void PickupItem(Entity item)
        {
            var comp = item.GetComponentOrNull<Item>();
            if (comp == null || !comp.CanPickup(Entity)) return;

            if (comp.OnPickup(Entity)) Entity.AddChild(item);
        }

        protected virtual void OnHealed(object sender, HealedEventArgs e) { }

        protected virtual void OnDamaged(object sender, DamagedEventArgs e)
        {
            World.SplashBlood(Position2D, Math.Min(0.25f * e.Damage + 0.5f, 4.0f));
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
            trace.HitEntityPredicate = (x => x != Entity
                && (!x.HasComponent<Zombie>() || !Entity.HasComponent<Zombie>())
                && (!x.HasComponent<Survivor>() || !Entity.HasComponent<Survivor>()));
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

            _moveDir = dir.Normalized();

            Anim.Speed = MoveSpeed;
        }

        public void StopMoving()
        {
            if (Movement == null)
                return;

            if (!Anim.Playing || Movement.IsMoving || Anim.CurAnim != StandAnim)
                Anim.Start(StandAnim);

            _moveDir = Vector2.Zero;
        }

        public void Select()
        {
            if (IsSelected) return;

            var marker = Entity.Create(World, "selection marker");

            marker.Position2D = Entity.Position2D;

            Entity.AddChild(marker).Spawn();
        }

        public void Deselect()
        {
            if (!IsSelected) return;

            Entity.Children.First(x => x.ClassName == "selection marker").Remove();
        }

        public bool ToggleSelected()
        {
            if (IsSelected) {
                Deselect();
                return false;
            } else {
                Select();
                return true;
            }
        }

        public override void OnThink(double dt)
        {
            base.OnThink(dt);

            if (Movement == null) return;

            Movement.Velocity += (_moveDir * MoveSpeed - Movement.Velocity) * (float) Math.Min(1.0, 4.0 * dt);
            FaceDirection(Movement.Velocity);
        }
    }
}
