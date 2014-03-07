using System;
using System.Linq;
using System.Collections.Generic;

using OpenTK;
using Zombles.Geometry;
using Zombles.Entities;

namespace Zombles.Scripts.Entities.Desires
{
    class ThreatAvoidance : Desire
    {
        public static IEnumerable<ThreatAvoidance> Discover(Beliefs beliefs)
        {
            var ents = new List<EntityBeliefs>();
            foreach (var zom in beliefs.Entities.Where(x => x.Type == EntityType.Zombie)) {
                var diff = beliefs.Entity.World.Difference(zom.LastPos, beliefs.Entity.Position2D);

                if (diff.LengthSquared < 0.25f || diff.LengthSquared >= 64f) continue;

                ents.Add(zom);
            }

            if (ents.Count > 0) {
                yield return new ThreatAvoidance(ents);
            }
        }

        public IEnumerable<EntityBeliefs> Threats { get; private set; }

        public override float Utility
        {
            get { return Threats.Count(); }
        }

        public ThreatAvoidance(IEnumerable<EntityBeliefs> threats)
        {
            Threats = threats.ToArray();
        }

        public override bool ConflictsWith(Desire other)
        {
            return other is ThreatAvoidance;
        }

        public override Desire ResolveConflict(Desire other)
        {
            if (other is ThreatAvoidance) {
                return ResolveConflict((ThreatAvoidance) other);
            } else {
                return this;
            }
        }

        public ThreatAvoidance ResolveConflict(ThreatAvoidance other)
        {
            return new ThreatAvoidance(Threats.Union(other.Threats));
        }

        public override Intention GetIntention(Beliefs beliefs)
        {
            return new Intentions.ThreatAvoidance(this, beliefs);
        }
    }
}
