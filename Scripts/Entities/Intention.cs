using System.Collections.Generic;

using OpenTK;

using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    public abstract class Action : Conflictable<Action>
    {
        public abstract void Perform(Human agent);
    }

    public class MovementAction : Action
    {
        public Vector2 Vector { get; private set; }

        public override float Utility
        {
            get { return Vector.Length; }
        }

        public MovementAction(Vector2 vector)
        {
            Vector = vector;
        }

        public override bool ConflictsWith(Action other)
        {
            return other is MovementAction;
        }

        public override Action ResolveConflict(Action other)
        {
            var move = (MovementAction) other;
            return new MovementAction(move.Vector + Vector);
        }

        public override void Perform(Human agent)
        {
            agent.StartMoving(Vector);
        }
    }

    public class AttackAction : Action
    {
        private float _utility;

        public Entity Target { get; private set; }

        public override float Utility
        {
            get { return _utility; }
        }

        public AttackAction(Entity target)
        {
            Target = target;

            var health = target.GetComponentOrNull<Health>();

            if (health == null) {
                _utility = 0f;
            } else {
                _utility = (float) (health.MaxHealth - health.Value) / health.MaxHealth;

                if (Target.HasComponent<Zombie>()) {
                    _utility += 2f;
                } else if (Target.HasComponent<WoodenBreakable>()) {
                    _utility += 1f;
                }
            }
        }

        public override bool ConflictsWith(Action other)
        {
            return other is AttackAction;
        }

        public override Action ResolveConflict(Action other)
        {
            return this;
        }

        public override void Perform(Human agent)
        {
            agent.Attack(Target.World.Difference(agent.Entity.Position2D, Target.Position2D));
        }
    }

    public class PickupItemAction : Action
    {
        private Entity _item;
        private float _utility;

        public PickupItemAction(Entity item, float utility)
        {
            _item = item;
            _utility = utility;
        }

        public override void Perform(Human agent)
        {
            agent.PickupItem(_item);
        }

        public override float Utility
        {
            get { return _utility; }
        }

        public override bool ConflictsWith(Action other)
        {
            return other is PickupItemAction || other is DropItemAction;
        }

        public override Action ResolveConflict(Action other)
        {
            return this;
        }
    }

    public class DropItemAction : Action
    {
        private float _utility;

        public DropItemAction(float utility)
        {
            _utility = utility;
        }

        public override void Perform(Human agent)
        {
            agent.DropItem();
        }

        public override float Utility
        {
            get { return _utility; }
        }

        public override bool ConflictsWith(Action other)
        {
            return other is PickupItemAction || other is DropItemAction;
        }

        public override Action ResolveConflict(Action other)
        {
            return this;
        }
    }

    public abstract class Intention
    {
        public Desire Desire { get; private set; }

        public Beliefs Beliefs { get; private set; }

        protected Human Human { get { return Beliefs.Human; } }

        protected Entity Entity { get { return Beliefs.Entity; } }

        protected World World { get { return Beliefs.Entity.World; } }

        public Intention(Desire desire, Beliefs beliefs)
        {
            Desire = desire;
            Beliefs = beliefs;
        }

        internal void Abandon()
        {
            OnAbandon();
        }

        public abstract bool ShouldAbandon();

        public abstract bool ShouldKeep();

        public abstract IEnumerable<Action> GetActions();

        protected virtual void OnAbandon();
    }
}
