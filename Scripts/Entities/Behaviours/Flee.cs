using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class Flee<T> : SubsumptionStack.Layer
        where T : Component
    {
        public double MinFleeCheckPeriod { get; set; }
        public double MaxFleeCheckPeriod { get; set; }

        public float FleeRadius { get; set; }
        public float RunRadius { get; set; }

        private double _nextFleeCheck;
        private Vector2 _fleeDir;

        protected override void OnSpawn()
        {
            _nextFleeCheck = MainWindow.Time;
            _fleeDir = new Vector2();

            MinFleeCheckPeriod = 0.25;
            MaxFleeCheckPeriod = 0.5;

            FleeRadius = 4.0f + Tools.Random.NextSingle() * 6.0f;
            RunRadius = 2.0f + Tools.Random.NextSingle() * 4.0f;
        }

        protected override bool OnThink(double dt)
        {
            if (MainWindow.Time >= _nextFleeCheck) {
                _nextFleeCheck = MainWindow.Time + Tools.Random.NextDouble(MinFleeCheckPeriod, MaxFleeCheckPeriod);

                _fleeDir = new Vector2();

                var trace = new Trace(City);
                trace.Origin = Position2D;
                trace.HitGeometry = true;
                trace.HitEntities = false;

                var it = SearchNearbyEnts(FleeRadius);
                while (it.MoveNext()) {
                    Entity cur = it.Current;
                    if (!cur.HasComponent<T>()) continue;

                    Vector2 diff = City.Difference(Position2D, cur.Position2D);
                    float dist2 = diff.LengthSquared;

                    if (dist2 == 0) continue;

                    trace.Target = cur.Position2D;

                    if (trace.GetResult().Hit) continue;

                    _fleeDir -= diff / dist2;

                    if (dist2 < RunRadius * RunRadius && Human is Survivor) {
                        (Human as Survivor).StartRunning();
                    }
                }
            }

            if (_fleeDir.LengthSquared > 0f) {
                Human.StartMoving(_fleeDir);
                return true;
            } else {
                return false;
            }
        }
    }
}
