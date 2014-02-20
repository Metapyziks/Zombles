using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public abstract class Intention
    {
        public Desire Desire { get; private set; }

        public Beliefs Beliefs { get; private set; }

        protected Human Human { get { return Beliefs.Human; } }

        protected Entity Entity { get { return Beliefs.Entity; } }

        public Intention(Desire desire, Beliefs beliefs)
        {
            Desire = desire;
            Beliefs = beliefs;
        }

        public abstract bool ShouldAbandon();

        public abstract bool ShouldKeep();

        public abstract void Act();
    }
}
