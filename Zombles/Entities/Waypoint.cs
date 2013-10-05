﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Geometry;

namespace Zombles.Entities
{
    public struct PathEdge
    {
        public readonly Waypoint StartWaypoint;
        public readonly Waypoint EndWaypoint;

        public readonly Vector2 Origin;
        public readonly Vector2 Vector;
        public readonly float Length;

        public PathEdge(Waypoint startWaypoint, Waypoint endWaypoint, Vector2 vector)
        {
            StartWaypoint = startWaypoint;
            EndWaypoint = endWaypoint;

            Origin = startWaypoint.Entity.Position2D;
            Vector = vector;

            Length = Vector.Length;
        }
    }

    public class PathGroup : IEnumerable<Waypoint>
    {
        private static int _sNextGroupID = 0;

        public int GroupID { get; private set; }
        public List<Waypoint> Waypoints { get; private set; }

        public IEnumerator<Waypoint> GetEnumerator()
        {
            return Waypoints.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Waypoints.GetEnumerator();
        }

        public PathGroup()
        {
            GroupID = _sNextGroupID++;
            Waypoints = new List<Waypoint>();
        }

        public void Merge(PathGroup other)
        {
            if (other.GroupID == GroupID)
                return;

            if (other.GroupID < GroupID) {
                other.Merge(this);
                return;
            }

            foreach (Waypoint waypoint in other)
                Add(waypoint);

            other.Waypoints.Clear();
        }

        public void Add(Waypoint waypoint)
        {
            Waypoints.Add(waypoint);
            waypoint.Group = this;
        }
    }

    public class Waypoint : Component
    {
        public const int NetworkQuality = 65536;
        public const float ConnectionRadius = 4.0f;
        public static readonly Vector2 ConnectionHullSize = new Vector2(0.5f, 0.5f);

        private static List<Vector2> _sHints = new List<Vector2>();

        public static void AddHint(Vector2 pos)
        {
            _sHints.Add(pos);
        }

        public static bool ShouldPlace(City city, Vector2 pos, bool hint)
        {
            PathGroup group = null;
            int found = 0;
            bool close = false;

            var near = new NearbyEntityEnumerator(city, pos, ConnectionRadius);
            while (near.MoveNext()) {
                if (near.Current.HasComponent<Waypoint>()) {
                    var waypoint = near.Current.GetComponent<Waypoint>();

                    if (waypoint.Position2D.Equals(pos))
                        return false;

                    var res = Trace.Quick(city, pos, waypoint.Position2D, false, true, ConnectionHullSize);

                    if (res.Hit)
                        res = Trace.Quick(city, waypoint.Position2D, pos, false, true, ConnectionHullSize);

                    if (!res.Hit) {
                        if (res.Vector.LengthSquared < 1.0f ||
                            !hint && res.Vector.LengthSquared < 4.0f)
                            close = true;

                        if (group == null)
                            group = waypoint.Group;
                        else if (group.GroupID != waypoint.Group.GroupID)
                            return true;

                        ++found;
                    }
                }
            }

            if (close)
                return false;

            return true;
        }

        public static void GenerateNetwork(City city, int seed = 0)
        {
            var rand = seed == 0 ? new Random() : new Random(seed);

            int count = 0;

            int tries = 0;
            while (tries++ < NetworkQuality) {
                Vector2 pos;
                bool hint = false;

                if (_sHints.Count > 0) {
                    pos = _sHints.Last();
                    _sHints.RemoveAt(_sHints.Count - 1);
                    hint = true;
                } else {
                    pos = new Vector2((int) (rand.NextSingle() * city.Width) + 0.5f,
                        (int) (rand.NextSingle() * city.Height) + 0.5f);
                }

                if (ShouldPlace(city, pos, hint)) {
                    var waypoint = Entity.Create(city, "waypoint");
                    waypoint.Position2D = pos;
                    waypoint.Spawn();

                    ++count;
                    tries = 0;
                }
            }
        }

        public PathGroup Group { get; set; }
        public List<PathEdge> Connections { get; private set; }

        public Waypoint(Entity ent)
            : base(ent)
        {
            Group = null;
            Connections = null;
        }

        public bool IsConnected(Waypoint waypoint)
        {
            return Connections.Exists(x => x.EndWaypoint == waypoint);
        }

        public override void OnSpawn()
        {
            Connections = new List<PathEdge>();

            var near = new NearbyEntityEnumerator(City, Position2D, ConnectionRadius);
            while (near.MoveNext()) {
                if (near.Current == Entity)
                    continue;

                if (near.Current.HasComponent<Waypoint>()) {
                    var other = near.Current.GetComponent<Waypoint>();

                    var res = Trace.Quick(City, Position2D, other.Position2D, false, true, ConnectionHullSize);
                    if (res.Hit)
                        res = Trace.Quick(City, other.Position2D, Position2D, false, true, ConnectionHullSize);

                    if (!res.Hit) {
                        Connections.Add(new PathEdge(this, other, res.Vector));
                        other.Connections.Add(new PathEdge(other, this, res.Vector));

                        if (Group == null)
                            other.Group.Add(this);
                        else if (Group != other.Group)
                            other.Group.Merge(Group);
                    }
                }
            }

            if (Group == null) {
                Group = new PathGroup();
                Group.Add(this);
            }
        }
    }
}

