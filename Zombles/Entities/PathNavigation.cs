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
        private Path myCurrentPath;
        private int myPathProgress;

        private double myLastScanTime;

        public bool HasPath
        {
            get { return myCurrentPath != null; }
        }

        public Path CurrentPath
        {
            get
            {
                return myCurrentPath;
            }
            set
            {
                myCurrentPath = value;
                myPathProgress = 0;
                myLastScanTime = ZomblesGame.Time - 1.0;
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

                if ( myPathProgress < CurrentPath.Waypoints.Length )
                    return CurrentPath.Waypoints[ myPathProgress ].Entity.Position2D;

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
                    myPathProgress++;
                    if ( myPathProgress > CurrentPath.Waypoints.Length )
                        CurrentPath = null;
                    else
                        ScanAhead();
                }
                else if ( ( ZomblesGame.Time - myLastScanTime ) >= 1.0 )
                    ScanAhead();
            }
        }

        private void ScanAhead()
        {
            myLastScanTime = ZomblesGame.Time;

            Trace trace = new Trace( City )
            {
                Origin = Position2D,
                HitEntities = false,
                HitGeometry = true,
                HullSize = new Vector2( 0.5f, 0.5f )
            };

            int best = myPathProgress;
            for ( ; myPathProgress <= CurrentPath.Waypoints.Length; ++myPathProgress )
            {
                trace.Target = NextWaypoint;
                if ( !trace.GetResult().Hit )
                    best = myPathProgress;
            }

            myPathProgress = best;
        }
    }
}
