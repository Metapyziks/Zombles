namespace Zombles.Scripts.Entities
{
    public abstract class Desire : Conflictable<Desire>
    {
        public abstract Intention GetIntention(Beliefs beliefs);

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
