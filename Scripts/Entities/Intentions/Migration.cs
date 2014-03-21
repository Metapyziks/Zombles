using System.Collections.Generic;
using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Intentions
{
    class Migration : Intention
    {
        private Block _destBlock;
        private Vector2 _destPos;
        private RouteNavigator _nav;

        public Migration(Desires.Migration desire, Beliefs beliefs)
            : base(desire, beliefs)
        {
            _destBlock = desire.Destination;

            var size = new Vector2(_destBlock.Width, _destBlock.Height);

            int tries = 16;
            do {
                _destPos = new Vector2(_destBlock.X, _destBlock.Y) + size * 0.33f + new Vector2(
                    Tools.Random.NextSingle(0f, size.X * 0.33f),
                    Tools.Random.NextSingle(0f, size.Y * 0.33f));
            } while (--tries > 0 && !Beliefs.Entity.World.IsPositionNavigable(_destPos));

            if (tries <= 0) {
                _destPos = _destBlock.GetNearestPosition(Entity.Position2D);
                _destPos += (_destBlock.Center - _destPos).Normalized() * 2f;
            }

            _nav = new RouteNavigator(Entity, _destPos);
        }

        public override bool ShouldAbandon()
        {
            return _destBlock == Entity.Block;
        }

        public override bool ShouldKeep()
        {
            return !ShouldAbandon();
        }

        protected override void OnAbandon()
        {
            _nav.Dispose();
        }

        public override IEnumerable<Action> GetActions()
        {
            if (_nav.HasDirection) {
                if (Human.IsHoldingItem) {
                    yield return new DropItemAction(1f);
                }

                yield return new MovementAction(_nav.GetDirection() * Desire.Utility);
            }
        }
    }
}
