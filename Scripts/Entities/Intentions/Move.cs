using OpenTK;
using Zombles.Scripts.Entities.Desires;

namespace Zombles.Scripts.Entities.Intentions
{
    class Move : Intention
    {
        private Vector2 _direction;

        public Move(Avoidance desire, Beliefs beliefs)
            : base(desire, beliefs)
        {
            _direction = desire.Vector.Normalized();
        }

        public override bool ShouldAbandon()
        {
            return false;
        }

        public override bool ShouldKeep()
        {
            return false;    
        }

        public override void Act()
        {
            Human.StartMoving(_direction);
        }
    }
}
