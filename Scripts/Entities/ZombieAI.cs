using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    public class ZombieAI : HumanControl
    {
        private const double TargetSearchInterval = 1.0;

        private float _viewRadius;

        private double _lastSearch;
        private double _lastSeen;
        private Entity _curTarget;
        private Vector2 _lastSeenPos;

        public ZombieAI(Entity ent)
            : base(ent)
        {
            _viewRadius = 12.0f;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            _lastSearch = MainWindow.Time - Tools.Random.NextDouble() * TargetSearchInterval;
            _curTarget = null;
        }

        public override void OnThink(double dt)
        {
            if (!Human.Health.IsAlive)
                return;

            if (MainWindow.Time - _lastSearch > TargetSearchInterval)
                FindTarget();

            if (_curTarget != null) {
                Vector2 diff = World.Difference(Position2D, _curTarget.Position2D);

                Health targHealth = _curTarget.GetComponent<Health>();

                if (!_curTarget.HasComponent<Survivor>() || !targHealth.IsAlive || diff.LengthSquared > _viewRadius * _viewRadius)
                    _curTarget = null;
                else {
                    _lastSeenPos = _curTarget.Position2D;
                    _lastSeen = MainWindow.Time;

                    if (Human.CanAttack) {
                        if (diff.LengthSquared < 0.75f) {
                            Human.Attack(diff);
                            Human.StopMoving();
                        } else {
                            Human.StartMoving(diff);
                        }
                    }
                }
            } else {
                if ((MainWindow.Time - _lastSeen) > 10.0 ||
                    World.Difference(Position2D, _lastSeenPos).LengthSquared <= 1.0f) {
                    int attempts = 0;
                    while (attempts++ < 16) {
                        float rad = 2.0f + Tools.Random.NextSingle() * 6.0f;
                        double ang = Tools.Random.NextDouble() * Math.PI * 2.0;

                        _lastSeenPos = new Vector2(
                            Position2D.X + (float) Math.Cos(ang) * rad,
                            Position2D.Y + (float) Math.Sin(ang) * rad
                        );

                        TraceLine trace = new TraceLine(World);
                        trace.Origin = Position2D;
                        trace.Target = _lastSeenPos;
                        trace.HitGeometry = true;
                        trace.HitEntities = false;
                        trace.HullSize = Entity.GetComponent<Collision>().Size;

                        if (!trace.GetResult().Hit)
                            break;
                    }

                    if (attempts == 16)
                        _lastSeen = MainWindow.Time + Tools.Random.NextDouble() * 1.0 + 9.0;
                    else
                        _lastSeen = MainWindow.Time;
                }

                Human.StartMoving(World.Difference(Position2D, _lastSeenPos));
            }
        }

        private void FindTarget()
        {
            TraceLine trace = new TraceLine(World);
            trace.Origin = Position2D;
            trace.HitGeometry = true;
            trace.HitEntities = false;
            trace.HullSize = Entity.GetComponent<Collision>().Size;

            Entity closest = null;
            float bestDist2 = _viewRadius * _viewRadius;

            NearbyEntityEnumerator it = SearchNearbyEnts(_viewRadius);
            while (it.MoveNext()) {
                if (!it.Current.HasComponent<Survivor>())
                    continue;

                if (!it.Current.GetComponent<Health>().IsAlive)
                    continue;

                Vector2 diff = World.Difference(Position2D, it.Current.Position2D);

                float dist2 = diff.LengthSquared;
                if (dist2 < bestDist2) {
                    trace.Target = it.Current.Position2D;

                    if (!trace.GetResult().Hit) {
                        closest = it.Current;
                        bestDist2 = dist2;
                    }
                }
            }

            _curTarget = closest;
            _lastSearch = MainWindow.Time;
        }
    }
}
