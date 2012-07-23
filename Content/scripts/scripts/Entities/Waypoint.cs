using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    public struct PathEdge
    {
        public readonly Vector2 Origin;
        public readonly Vector2 Vector;
        public readonly float Length;

        public PathEdge( Waypoint waypoint, Vector2 vector )
        {
            Origin = waypoint.Entity.Position2D;
            Vector = vector;

            Length = Vector.Length;
        }
    }

    public class Waypoint : Component
    {
        public List<PathEdge> Connections { get; private set; }

        public Waypoint( Entity ent )
            : base( ent )
        {
            Connections = new List<PathEdge>();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            float maxRadius = 16.0f;
            NearbyEntityEnumerator near = new NearbyEntityEnumerator( City, Position2D, maxRadius );
            while ( near.MoveNext() )
            {
                if ( near.Current.HasComponent<Waypoint>() )
                {
                    Waypoint other = near.Current.GetComponent<Waypoint>();

                    Trace tr = new Trace( City );
                    tr.Origin = Position2D;
                    tr.Target = other.Position2D;
                    tr.HitEntities = false;
                    tr.HitGeometry = true;

                    TraceResult res = tr.GetResult();
                    if ( !res.Hit )
                        Connections.Add( new PathEdge( this, res.Vector ) );

                    tr.Origin = other.Position2D;
                    tr.Target = Position2D;

                    res = tr.GetResult();
                    if ( !res.Hit )
                        other.Connections.Add( new PathEdge( other, res.Vector ) );
                }
            }
        }
    }
}

