using System;
using System.Collections.Generic;
using OpenTK;
using Zombles.Geometry;

namespace Zombles.Entities
{
    public class RouteNavigator : IDisposable
    {
        private static double MaxRouteFindingTime = 1.0 / 120.0;

        private static Queue<RouteNavigator> _sQueue = new Queue<RouteNavigator>();

        public static void Think(double dt)
        {
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            while (_sQueue.Count > 0) {
                var first = _sQueue.Dequeue();
                first.CalculatePath();

                if (timer.Elapsed.TotalSeconds >= MaxRouteFindingTime)
                    break;
            }
        }

        private Entity _entity;
        private Route _route;

        private List<Vector2> _history;
        private IEnumerator<Vector2> _curPath;
        private Vector2 _curWaypoint;

        private bool _ended;
        private bool _disposed;

        private double _lastScan;

        public bool HasPath
        {
            get { return _curPath != null; }
        }

        public bool HasEnded
        {
            get { return _ended; }
        }

        public bool HasDirection
        {
            get { return _curPath != null && !_disposed; }
        }

        public Route Route
        {
            get
            {
                return _route;
            }
        }

        public Vector2 CurrentTarget
        {
            get
            {
                if (!HasPath)
                    return _entity.Position2D;

                return Route.Target;
            }
        }

        private Vector2 NextWaypoint
        {
            get
            {
                if (!HasPath || _disposed)
                    return _entity.Position2D;

                if (!_ended)
                    return _curWaypoint;

                return Route.Target;
            }
        }

        public RouteNavigator(Entity ent, Vector2 dest)
        {
            _entity = ent;

            NavigateTo(dest);
        }

        private void NavigateTo(Vector2 target)
        {
            _route = Route.Find(_entity.World, _entity.Position2D, target);
            _curPath = null;

            if (!_sQueue.Contains(this)) {
                _sQueue.Enqueue(this);
            }
        }

        private void CalculatePath()
        {
            if (!_disposed && !HasPath) {
                _history = new List<Vector2>();

                _curPath = _route.GetEnumerator();
                _curWaypoint = _route.Origin;
                _ended = !_curPath.MoveNext();

                if (!_ended) {
                    ScanAhead();
                }
            }
        }

        public Vector2 GetDirection()
        {
            if (!_disposed && !_ended && HasPath) {
                if ((NextWaypoint - _entity.Position2D).LengthSquared <= 0.25f) {
                    MoveNext();
                } else if ((MainWindow.Time - _lastScan) >= 1.0) {
                    ScanAhead();
                }
            }

            var diff = _entity.World.Difference(_entity.Position2D, NextWaypoint);

            if (diff.LengthSquared == 0) return diff;

            return diff.Normalized();
        }

        private void MoveNext()
        {
            if (_ended || _disposed) {
                return;
            }

            _history.Add(_curWaypoint);

            _curWaypoint = _curPath.Current;
            _ended = !_curPath.MoveNext();

            if (!_ended) {
                ScanAhead();
            }
        }

        private void ScanAhead()
        {
            if (_ended || _disposed) return;
            if (!_entity.HasComponent<Collision>()) return;

            _lastScan = MainWindow.Time;

            TraceLine trace = new TraceLine(_entity.World) {
                Origin = _entity.Position2D,
                Target = _curPath.Current,
                HitEntities = false,
                HitGeometry = true,
                HullSize = _entity.GetComponent<Collision>().Size * 0.95f
            };

            if (!trace.GetResult().Hit) {
                MoveNext();
            } else {
                trace.Target = _curWaypoint;
                if (trace.GetResult().Hit) {
                    if (_entity.World.IsPositionNavigable(CurrentTarget)) {
                        NavigateTo(CurrentTarget);
                    } else {
                        _ended = true;
                    }
                }
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
