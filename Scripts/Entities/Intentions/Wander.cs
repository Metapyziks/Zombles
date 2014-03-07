using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Intentions
{
    public class Wander : Intention
    {
        private Vector2 _curDest;
        private double _nextRandomize;

        public Wander(Desires.Wander desire, Beliefs beliefs)
            : base(desire, beliefs)
        {
            Randomize();
        }

        private void Randomize()
        {
            _curDest = Entity.Position2D;

            var trace = new TraceLine(World) {
                HitEntities = false,
                HitGeometry = true
            };

            float best = 0f;
            for (int i = 0; i < 16; ++i) {
                var ang = Tools.Random.NextSingle(0f, MathHelper.TwoPi);
                trace.Vector = new Vector2((float) Math.Cos(ang), (float) Math.Sin(ang)) * 4f;
                var res = trace.GetResult();

                if (res.Vector.LengthSquared > best) {
                    _curDest = Entity.Position2D + res.Vector;
                }
            }

            _nextRandomize = MainWindow.Time + 1.0 + Tools.Random.NextDouble() * 2.0;
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
            yield return new MovementAction(World.Difference(Entity.Position2D, _curDest));
        }
    }
}
