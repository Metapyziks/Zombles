using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using Zombles.Geometry;

namespace Zombles.Entities
{
    public class PathNavigation : Component
    {
        private Path _currentPath;
        private int _pathProgress;

        private double _lastScanTime;

        public bool HasPath
        {
            get { return _currentPath != null; }
        }

        public Path CurrentPath
        {
            get
            {
                return _currentPath;
            }
            set
            {
                _currentPath = value;
                _pathProgress = 0;
                _lastScanTime = ZomblesGame.Time - 1.0;
            }
        }

        public Vector2 CurrentTarget
        {
            get
            {
                if ( CurrentPath == null )
                    return Entity.Position2D;

                return CurrentPath.Desination;
            }
        }

        public Vector2 NextWaypoint
        {
            get
            {
                if ( CurrentPath == null )
                    return Entity.Position2D;

                if ( _pathProgress < CurrentPath.Waypoints.Length )
                    return CurrentPath.Waypoints[ _pathProgress ].Entity.Position2D;

                return CurrentPath.Desination;
            }
        }

        public PathNavigation( Entity ent )
            : base( ent )
        {

        }

        public void NavigateTo( Vector2 dest )
        {
            CurrentPath = Path.Find( City, Entity.Position2D, dest );
        }

        public override void OnThink( double dt )
        {
            if ( CurrentPath != null )
            {
                if ( ( NextWaypoint - Position2D ).LengthSquared <= 0.25f )
                {
                    _pathProgress++;
                    if ( _pathProgress > CurrentPath.Waypoints.Length )
                        CurrentPath = null;
                    else
                        ScanAhead();
                }
                else if ( ( ZomblesGame.Time - _lastScanTime ) >= 1.0 )
                    ScanAhead();
            }
        }

        private void ScanAhead()
        {
            _lastScanTime = ZomblesGame.Time;

            Trace trace = new Trace( City )
            {
                Origin = Position2D,
                HitEntities = false,
                HitGeometry = true,
                HullSize = new Vector2( 0.5f, 0.5f )
            };

            int best = _pathProgress;
            for ( ; _pathProgress <= CurrentPath.Waypoints.Length; ++_pathProgress )
            {
                trace.Target = NextWaypoint;
                if ( !trace.GetResult().Hit )
                    best = _pathProgress;
            }

            _pathProgress = best;
        }
    }
}
