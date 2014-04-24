using System;
using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class Flee : SubsumptionStack.Layer
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

            MinFleeCheckPeriod = 0.0;
            MaxFleeCheckPeriod = 1.0 / 16.0;

            FleeRadius = 4.0f + Tools.Random.NextSingle() * 6.0f;
            RunRadius = 2.0f + Tools.Random.NextSingle() * 4.0f;
        }

        protected override bool OnThink(double dt)
        {
            if (MainWindow.Time >= _nextFleeCheck) {
                _nextFleeCheck = MainWindow.Time + Tools.Random.NextDouble(MinFleeCheckPeriod, MaxFleeCheckPeriod);

                _fleeDir = new Vector2();

                foreach (var ent in SearchNearbyVisibleEnts(FleeRadius, (ent, diff) => 
                    ent.HasComponent<Zombie>() &&
                    ent.GetComponent<Health>().IsAlive &&
                    diff.LengthSquared > 0)) {

                    Vector2 diff = World.Difference(Position2D, ent.Position2D);
                    var dist2 = diff.LengthSquared;

                    _fleeDir -= diff / dist2;

                    if (dist2 < RunRadius * RunRadius && Human is Survivor) {
                        (Human as Survivor).StartRunning();
                    }
                }
            }

            if (_fleeDir.LengthSquared > 0f) {
                var tile = World.GetTile(Position2D);
                var wallAvoid = new Vector2();

                if (tile.IsWallSolid(Face.West) && !tile.IsWallSolid(Face.East)) {
                    wallAvoid.X += 1f - (Position2D.X - Mathf.Floor(Position2D.X));
                } else if (tile.IsWallSolid(Face.East) && !tile.IsWallSolid(Face.West)) {
                    wallAvoid.X -= Position2D.X - Mathf.Floor(Position2D.X);
                }

                if (tile.IsWallSolid(Face.North) && !tile.IsWallSolid(Face.South)) {
                    wallAvoid.Y += 1f - (Position2D.Y - Mathf.Floor(Position2D.Y));
                } else if (tile.IsWallSolid(Face.South) && !tile.IsWallSolid(Face.North)) {
                    wallAvoid.Y -= Position2D.Y - Mathf.Floor(Position2D.Y);
                }

                Human.StartMoving(_fleeDir + wallAvoid);
                return true;
            } else {
                return false;
            }
        }
    }
}
