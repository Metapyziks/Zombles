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
        private const double AttackInterval = 1.0;

        private float _viewRadius;

        private double _lastSearch;
        private double _lastAttack;
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

            _lastSearch = ZomblesGame.Time - Tools.Random.NextDouble() * TargetSearchInterval;
            _curTarget = null;
        }

        public override void OnThink(double dt)
        {
            if (!Human.Health.IsAlive)
                return;

            if (ZomblesGame.Time - _lastSearch > TargetSearchInterval)
                FindTarget();

            if (_curTarget != null) {
                Vector2 diff = City.Difference(Position2D, _curTarget.Position2D);

                Health targHealth = _curTarget.GetComponent<Health>();

                if (!_curTarget.HasComponent<Survivor>() || !targHealth.IsAlive || diff.LengthSquared > _viewRadius * _viewRadius)
                    _curTarget = null;
                else {
                    _lastSeenPos = _curTarget.Position2D;
                    _lastSeen = ZomblesGame.Time;

                    if (ZomblesGame.Time - _lastAttack > AttackInterval) {
                        if (diff.LengthSquared < 0.75f) {
                            Human.Attack(diff);
                            _lastAttack = ZomblesGame.Time;
                            Human.StopMoving();
                        } else {
                            Human.StartMoving(diff);
                        }
                    }
                }
            } else {
                if ((ZomblesGame.Time - _lastSeen) > 10.0 ||
                    City.Difference(Position2D, _lastSeenPos).LengthSquared <= 1.0f) {
                    int attempts = 0;
                    while (attempts++ < 16) {
                        float rad = 2.0f + Tools.Random.NextSingle() * 6.0f;
                        double ang = Tools.Random.NextDouble() * Math.PI * 2.0;

                        _lastSeenPos = new Vector2(
                            Position2D.X + (float) Math.Cos(ang) * rad,
                            Position2D.Y + (float) Math.Sin(ang) * rad
                        );

                        Trace trace = new Trace(City);
                        trace.Origin = Position2D;
                        trace.Target = _lastSeenPos;
                        trace.HitGeometry = true;
                        trace.HitEntities = false;

                        if (!trace.GetResult().Hit)
                            break;
                    }

                    if (attempts == 16)
                        _lastSeen = ZomblesGame.Time + Tools.Random.NextDouble() * 1.0 + 9.0;
                    else
                        _lastSeen = ZomblesGame.Time;
                }

                Human.StartMoving(City.Difference(Position2D, _lastSeenPos));
            }
        }

        private void FindTarget()
        {
            Trace trace = new Trace(City);
            trace.Origin = Position2D;
            trace.HitGeometry = true;
            trace.HitEntities = false;

            Entity closest = null;
            float bestDist2 = _viewRadius * _viewRadius;

            NearbyEntityEnumerator it = SearchNearbyEnts(_viewRadius);
            while (it.MoveNext()) {
                if (!it.Current.HasComponent<Survivor>())
                    continue;

                if (!it.Current.GetComponent<Health>().IsAlive)
                    continue;

                Vector2 diff = City.Difference(Position2D, it.Current.Position2D);

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
            _lastSearch = ZomblesGame.Time;
        }
    }
}
