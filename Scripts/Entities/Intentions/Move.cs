using OpenTK;

namespace Zombles.Scripts.Entities.Intentions
{
    class Move : Intention
    {
        private Vector2 _direction;

        public Move(Beliefs beliefs, Vector2 direction)
            : base(beliefs)
        {
            _direction = direction;
        }

        public override bool ShouldAbandon()
        {
            return false;
        }

        public override void Act()
        {
            Human.StartMoving(_direction);
        }
    }
}
