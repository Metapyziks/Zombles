using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public abstract class Intention
    {
        public Beliefs Beliefs { get; private set; }

        protected Human Human { get { return Beliefs.Human; } }

        protected Entity Entity { get { return Beliefs.Entity; } }

        public Intention(Beliefs beliefs)
        {
            Beliefs = beliefs;
        }

        public abstract bool ShouldAbandon();

        public abstract void Act();
    }
}
