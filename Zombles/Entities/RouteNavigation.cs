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
        private Route _curRoute;
        private IEnumerator<Vector2> _curProgress;
        private Vector2 _curWaypoint;
        private bool _ended;

        private double _lastScan;

        public bool HasPath
        {
            get { return _curRoute != null; }
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

                if (_curRoute != null) {
                    _curProgress = _curRoute.GetEnumerator();
                    _curWaypoint = _curRoute.Origin;
                    _ended = !_curProgress.MoveNext();

                    if (!_ended) {
                        ScanAhead();
                    }
                }
            }
        }

        public Vector2 CurrentTarget
        {
            get
            {
                if (CurrentRoute == null)
                    return Entity.Position2D;

                return CurrentRoute.Target;
            }
        }

        public Vector2 NextWaypoint
        {
            get
            {
                if (CurrentRoute == null)
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
            if (CurrentRoute != null) {
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
                return;
            }

            _curWaypoint = _curProgress.Current;
            _ended = !_curProgress.MoveNext();

            if (!_ended) {
                ScanAhead();
            }
        }

        private void ScanAhead()
        {
            _lastScan = MainWindow.Time;

            Trace trace = new Trace(City) {
                Origin = Position2D,
                Target = _curProgress.Current,
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
