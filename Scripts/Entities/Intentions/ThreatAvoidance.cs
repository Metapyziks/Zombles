using System;
using System.Collections.Generic;
using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;
using Zombles.Scripts.Entities;

namespace Zombles.Scripts.Entities.Intentions
{
    class ThreatAvoidance : Intention
    {
        private IEnumerable<EntityBeliefs> _threats;

        public ThreatAvoidance(Desires.ThreatAvoidance desire, Beliefs beliefs)
            : base(desire, beliefs)
        {
            _threats = desire.Threats;
        }

        public override bool ShouldAbandon()
        {
            return false;
        }

        public override bool ShouldKeep()
        {
            return true;
        }

        public override IEnumerable<Action> GetActions()
        {
            foreach (var zom in _threats) {
                var diff = World.Difference(zom.LastPos, Entity.Position2D);

                if (diff.LengthSquared < 1.5f) {
                    yield return new AttackAction(zom.Entity);
                }

                if (diff.LengthSquared < 6f) {
                    ((Survivor) Human).StartRunning();
                    yield return new DropItemAction(4f);
                }
                
                var timeSince = (float) Math.Max(1.0, MainWindow.Time - zom.LastSeen);

                var dir = diff.Normalized();
                var mag = 64f / diff.Length / (1f + timeSince * timeSince);

                yield return new MovementAction(dir * mag);
            }
        }
    }
}
