using System.Collections.Generic;

using OpenTK;

using Zombles.Scripts.Entities.Desires;

namespace Zombles.Scripts.Entities.Intentions
{
    class Move : Intention
    {
        private Vector2 _vector;

        public Move(Avoidance desire, Beliefs beliefs)
            : base(desire, beliefs)
        {
            _vector = desire.Vector;
        }

        public override bool ShouldAbandon()
        {
            return false;
        }

        public override bool ShouldKeep()
        {
            return false;    
        }

        public override IEnumerable<Action> GetActions()
        {
            yield return new MovementAction(_vector);
        }
    }
}
