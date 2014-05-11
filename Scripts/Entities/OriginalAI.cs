using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    public class OriginalAI : HumanControl
    {
        private const double TargetSearchInterval = 0.25;

        private float _fleeRadius;
        private float _runRadius;
        private float _fightRadius;

        private Vector2 _fleePos;
        private double _lastSearch;

        public OriginalAI(Entity ent)
            : base(ent)
        {
            _fleeRadius = 4.0f + Tools.Random.NextSingle() * 6.0f;
            _runRadius = 2.0f + Tools.Random.NextSingle() * 4.0f;
            _fightRadius = 0.75f + Tools.Random.NextSingle() * 1.25f;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            
            _fleePos = new Vector2();
            _lastSearch = MainWindow.Time - Tools.Random.NextDouble() * TargetSearchInterval;
        }

        public override void OnThink(double dt)
        {
            if (MainWindow.Time - _lastSearch > TargetSearchInterval) {
                _lastSearch = MainWindow.Time + Tools.Random.NextSingle() * TargetSearchInterval;

                _fleePos = new Vector2();

                var trace = new TraceLine(World);
                trace.Origin = Position2D;
                trace.HitGeometry = true;
                trace.HitEntities = false;

                NearbyEntityEnumerator it = SearchNearbyEnts(_fleeRadius);
                while (it.MoveNext()) {
                    Entity cur = it.Current;
                    if (cur.HasComponent<Zombie>() && cur.GetComponent<Health>().IsAlive) {
                        Vector2 diff = World.Difference(Position2D, cur.Position2D);
                        float dist2 = diff.LengthSquared;

                        if (dist2 > 0) {
                            trace.Target = cur.Position2D;

                            if (!trace.GetResult().Hit) {
                                if (dist2 < _fightRadius * _fightRadius) {
                                    _fleePos += diff / dist2;

                                    if (dist2 < 0.75f) {
                                        Human.Attack(diff);
                                        Human.StopMoving();
                                        return;
                                    }
                                } else {
                                    _fleePos -= diff / dist2;
                                }

                                if (dist2 < _runRadius * _runRadius)
                                    (Human as Survivor).StartRunning();
                            }
                        }
                    }
                }

                if (_fleePos.LengthSquared == 0.0f) {
                    _fleePos.X = Tools.Random.NextSingle() * 2.0f - 1.0f;
                    _fleePos.Y = Tools.Random.NextSingle() * 2.0f - 1.0f;
                }

                Human.StartMoving(_fleePos);
            }
        }
    }
}
