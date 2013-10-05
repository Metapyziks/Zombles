using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Geometry;

namespace Zombles.Entities
{
    public class Path
    {
        private class AStarNode
        {
            private float _heuristic;

            public readonly Waypoint Waypoint;
            public readonly AStarNode Previous;
            public readonly int Depth;
            public readonly float Cost;

            public float Heuristic
            {
                get { return _heuristic; }
                set
                {
                    _heuristic = value;
                    Total = Cost + value;
                }
            }
            public float Total { get; private set; }

            public AStarNode( Waypoint waypoint, Vector2 orig, Vector2 dest )
            {
                Waypoint = waypoint;
                Previous = null;
                Depth = 0;
                Cost = waypoint.Entity.City.Difference( waypoint.Entity.Position2D, orig ).Length;

                CalculateHeuristic( dest );
            }

            public AStarNode( Waypoint waypoint, AStarNode previous, float costAdd )
            {
                Waypoint = waypoint;
                Previous = previous;
                Depth = Previous.Depth + 1;
                Cost = Previous.Cost + costAdd;
            }

            public void CalculateHeuristic( Vector2 dest )
            {
                Heuristic = Waypoint.Entity.City.Difference( Waypoint.Entity.Position2D, dest ).Length;
            }
        }

        public static Path Find( City city, Vector2 origin, Vector2 destination )
        {
            Path path = new Path()
            {
                City = city,
                Origin = origin,
                Desination = destination
            };

            Waypoint first = FindNearestWaypoint( city, origin );
            Waypoint last = FindNearestWaypoint( city, destination );

            if ( first == null || last == null )
                return null; // TODO: Check to see if there is a straight path between the points anyway

            if ( first == last )
                path.Waypoints = new Waypoint[] { first };
            else
            {
                List<AStarNode> open = new List<AStarNode>();
                List<AStarNode> closed = new List<AStarNode>();

                open.Add( new AStarNode( first, origin, destination ) );

                while ( open.Count > 0 )
                {
                    AStarNode cur = null;
                    foreach ( AStarNode node in open )
                        if ( cur == null || cur.Total > node.Total )
                            cur = node;

                    open.Remove( cur );

                    if ( cur.Waypoint == last )
                    {
                        path.Waypoints = new Waypoint[ cur.Depth + 1 ];
                        for ( int i = cur.Depth; i >= 0; --i )
                        {
                            path.Waypoints[ i ] = cur.Waypoint;
                            cur = cur.Previous;
                        }
                        break;
                    }

                    foreach ( PathEdge edge in cur.Waypoint.Connections )
                    {
                        AStarNode node = new AStarNode( edge.EndWaypoint, cur, edge.Length );
                        AStarNode other = closed.FirstOrDefault( x => x.Waypoint == node.Waypoint );

                        if ( other != null )
                        {
                            if ( other.Cost <= node.Cost )
                                continue;

                            closed.Remove( other );

                            node.Heuristic = other.Heuristic;
                        }
                        else
                        {
                            other = open.FirstOrDefault( x => x.Waypoint == node.Waypoint );
                            if ( other != null )
                            {
                                if ( other.Cost <= node.Cost )
                                    continue;

                                open[ open.IndexOf( other ) ] = node;

                                node.Heuristic = other.Heuristic;
                            }
                            else
                            {
                                open.Add( node );
                                node.CalculateHeuristic( destination );
                            }
                        }
                    }

                    closed.Add( cur );
                }
            }

            if ( path.Waypoints == null )
                return null;

            return path;
        }

        private static Waypoint FindNearestWaypoint( City city, Vector2 vec )
        {
            NearbyEntityEnumerator near = new NearbyEntityEnumerator( city, vec, Waypoint.ConnectionRadius );
            Waypoint nearest = null;
            float nearDist2 = Waypoint.ConnectionRadius * Waypoint.ConnectionRadius;
            while ( near.MoveNext() )
            {
                if ( near.Current.HasComponent<Waypoint>() )
                {
                    float dist2 = ( near.Current.Position2D - vec ).LengthSquared;
                    if ( dist2 <= nearDist2 )
                    {
                        nearest = near.Current.GetComponent<Waypoint>();
                        nearDist2 = dist2;
                    }
                }
            }

            return nearest;
        }

        public City City { get; private set; }

        public Vector2 Origin { get; private set; }
        public Vector2 Desination { get; private set; }

        public Waypoint[] Waypoints { get; private set; }

        public Vector2 this[ int index ]
        {
            get { return Waypoints[index].Entity.Position2D; }
        }
    }
}
