using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;
using Zombles.Scripts.Entities.Desires;

namespace Zombles.Scripts.Entities.Intentions
{
    class Migrate : Intention
    {
        private Block _destBlock;
        private Vector2 _destPos;

        public Migrate(Migration desire, Beliefs beliefs)
            : base(desire, beliefs)
        {
            _destBlock = desire.Destination;

            var size = new Vector2(_destBlock.Width, _destBlock.Height);

            int tries = 4;
            do {
                _destPos = new Vector2(_destBlock.X, _destBlock.Y) + size * 0.25f + new Vector2(
                    Tools.Random.NextSingle(0f, size.X * 0.5f),
                    Tools.Random.NextSingle(0f, size.Y * 0.5f));
            } while (--tries > 0 && !Beliefs.Entity.World.IsPositionNavigable(_destPos));

            if (tries <= 0) {
                _destPos = _destBlock.GetNearestPosition(Entity.Position2D);
                _destPos += (_destBlock.Center - _destPos).Normalized() * 2f;
            }
        }

        public override bool ShouldAbandon()
        {
            return _destBlock == Entity.Block;
        }

        public override bool ShouldKeep()
        {
            return !ShouldAbandon();
        }

        public override IEnumerable<Action> GetActions()
        {
            var nav = Entity.GetComponent<RouteNavigation>();
            if (!nav.HasRoute || nav.CurrentTarget != _destPos) {
                nav.NavigateTo(_destPos);
            }

            if (nav.HasPath && nav.CurrentTarget == _destPos) {
                var diff = Entity.World.Difference(Entity.Position2D, nav.NextWaypoint);
                yield return new MovementAction(diff.Normalized() * Desire.Utility);
            }
        }
    }
}
