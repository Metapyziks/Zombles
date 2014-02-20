using System;
using System.Linq;
using System.Collections.Generic;

using OpenTK;

namespace Zombles.Scripts.Entities.Desires
{
    class Avoidance : Desire
    {
        public static IEnumerable<Avoidance> Discover(Beliefs beliefs)
        {
            foreach (var zom in beliefs.Entities.Where(x => x.Type == EntityType.Zombie)) {
                var diff = beliefs.Entity.World.Difference(zom.LastPos, beliefs.Entity.Position2D);

                if (diff.LengthSquared < 0.25f) continue;

                var timeSince = (float) Math.Max(1.0, MainWindow.Time - zom.LastSeen);

                var dir = diff.Normalized();
                var mag = 1f / diff.LengthSquared / (timeSince * timeSince);

                yield return new Avoidance(dir * mag);
            }
        }

        public Vector2 Vector { get; private set; }

        public override float Utility
        {
            get { return Vector.Length; }
        }

        public Avoidance(Vector2 vector)
        {
            Vector = vector;
        }

        public override bool ConflictsWith(Desire other)
        {
            return other is Avoidance;
        }

        public override Desire ResolveConflict(Desire other)
        {
            if (other is Avoidance) {
                return ResolveConflict((Avoidance) other);
            } else {
                return this;
            }
        }

        public Avoidance ResolveConflict(Avoidance other)
        {
            return new Avoidance(Vector + other.Vector);
        }

        public override Intention GetIntention(Beliefs beliefs)
        {
            return new Intentions.Move(this, beliefs);
        }
    }
}
