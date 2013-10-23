using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Zombles.Geometry;

namespace Zombles.Entities
{
    public class RouteNavigation : Component
    {
        private static double MaxRouteFindingTime = 1.0 / 120.0;

        private static Queue<RouteNavigation> _sQueue = new Queue<RouteNavigation>();

        public static void Think(double dt)
        {
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            while (_sQueue.Count > 0) {
                var first = _sQueue.Dequeue();
                if (first.HasRoute && !first.HasPath) {
                    first._curPath = first._curRoute.GetEnumerator();
                    first._curWaypoint = first._curRoute.Origin;
                    first._ended = !first._curPath.MoveNext();

                    if (!first._ended) {
                        first.ScanAhead();
                    }
                }

                if (timer.Elapsed.TotalSeconds >= MaxRouteFindingTime)
                    break;
            }
        }

        private Route _curRoute;
        private IEnumerator<Vector2> _curPath;
        private Vector2 _curWaypoint;
        private bool _ended;

        private double _lastScan;

        public bool HasRoute
        {
            get { return _curRoute != null; }
        }

        public bool HasPath
        {
            get { return _curPath != null; }
        }

        public Route CurrentRoute
        {
            get
            {
                return _curRoute;
            }
            set
            {
                _curRoute = value;
                _curPath = null;

                if (_curRoute != null && !_sQueue.Contains(this)) {
                    _sQueue.Enqueue(this);
                }
            }
        }

        public Vector2 CurrentTarget
        {
            get
            {
                if (!HasPath)
                    return Entity.Position2D;

                return CurrentRoute.Target;
            }
        }

        public Vector2 NextWaypoint
        {
            get
            {
                if (!HasPath)
                    return Entity.Position2D;

                if (!_ended)
                    return _curWaypoint;

                return CurrentRoute.Target;
            }
        }

        public RouteNavigation(Entity ent)
            : base(ent) { }

        public void NavigateTo(Vector2 target)
        {
            CurrentRoute = Route.Find(City, Entity.Position2D, target);
        }

        public override void OnThink(double dt)
        {
            if (HasPath) {
                if ((NextWaypoint - Position2D).LengthSquared <= 0.25f) {
                    MoveNext();
                } else if ((MainWindow.Time - _lastScan) >= 1.0) {
                    ScanAhead();
                }
            }
        }

        private void MoveNext()
        {
            if (_ended) {
                CurrentRoute = null;
                _curPath = null;
                return;
            }

            _curWaypoint = _curPath.Current;
            _ended = !_curPath.MoveNext();

            if (!_ended) {
                ScanAhead();
            }
        }

        private void ScanAhead()
        {
            if (_ended) return;

            _lastScan = MainWindow.Time;

            Trace trace = new Trace(City) {
                Origin = Position2D,
                Target = _curPath.Current,
                HitEntities = false,
                HitGeometry = true,
                HullSize = new Vector2(0.5f, 0.5f)
            };

            if (!trace.GetResult().Hit) {
                MoveNext();
            }
        }
    }
}
