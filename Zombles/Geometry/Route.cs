using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Zombles.Geometry
{
    public abstract class Route : IEnumerable<Vector2>
    {
        private class NodeInfo<T>
        {
            private float _heuristic;

            public T Node { get; private set; }
            public Vector2 Pos { get; private set; }
            public NodeInfo<T> Prev { get; private set; }
            public int Depth { get; private set; }
            public float Cost { get; private set; }
            public float Total { get; private set; }

            public float Heuristic
            {
                get { return _heuristic; }
                set
                {
                    _heuristic = value;
                    Total = Cost + value;
                }
            }

            public NodeInfo(T node, Vector2 pos)
            {
                Node = node;
                Pos = pos;
                Prev = null;
                Depth = 0;
                Cost = 0f;
            }

            public NodeInfo(T node, Vector2 pos, NodeInfo<T> prev, float costAdd)
            {
                Node = node;
                Pos = pos;
                Prev = prev;
                Depth = prev.Depth + 1;
                Cost = prev.Cost + costAdd;
            }

            public void CalculateHeuristic(City city, Vector2 target)
            {
                Heuristic = city.Difference(Pos, target).Length;
            }
        }

        private static IEnumerable<T> AStar<T>(City city, T origin, T target,
            Func<City, T, IEnumerable<Tuple<T, float>>> adjFunc, Func<T, Vector2> vecFunc)
        {
            var open = new List<NodeInfo<T>>();
            var clsd = new HashSet<NodeInfo<T>>();

            var targPos = vecFunc(target);

            var first = new NodeInfo<T>(origin, vecFunc(origin));
            first.CalculateHeuristic(city, targPos);

            open.Add(first);

            while (open.Count > 0) {
                NodeInfo<T> cur = null;
                foreach (var node in open) {
                    if (cur == null || node.Total < cur.Total) cur = node;
                }

                if (cur.Node.Equals(target)) {
                    var path = new T[cur.Depth + 1];
                    for (int i = cur.Depth; i >= 0; --i) {
                        path[i] = cur.Node;
                        cur = cur.Prev;
                    }
                    return path;
                }

                open.Remove(cur);
                clsd.Add(cur);

                foreach (var adj in adjFunc(city, cur.Node)) {
                    var node = new NodeInfo<T>(adj.Item1, vecFunc(adj.Item1), cur, adj.Item2);
                    var existing = clsd.FirstOrDefault(x => x.Node.Equals(adj.Item1));

                    if (existing != null) {
                        if (existing.Cost <= node.Cost) continue;

                        clsd.Remove(existing);
                        node.Heuristic = existing.Heuristic;
                    }

                    existing = open.FirstOrDefault(x => x.Node.Equals(adj.Item1));

                    if (existing != null) {
                        if (existing.Cost <= node.Cost) continue;

                        open.Remove(existing);
                        node.Heuristic = existing.Heuristic;
                    } else {
                        node.CalculateHeuristic(city, targPos);
                    }

                    open.Add(node);
                }
            }

            return null;
        }

        private class MacroRoute : Route
        {
            private static IEnumerable<Tuple<Intersection, float>> NeighboursFunc(City city, Intersection inter)
            {
                return inter.Edges.Select(x => Tuple.Create(x.Key, x.Value.Length));
            }

            private static Vector2 VectorFunc(Intersection inter)
            {
                return inter.Position;
            }

            public MacroRoute(City city, Vector2 origin, Vector2 target)
                : base(city, origin, target) { }

            public override IEnumerator<Vector2> GetEnumerator()
            {
                Intersection first = null;
                Intersection last = null;

                float fBest = float.MaxValue;
                float lBest = float.MaxValue;

                foreach (var inter in City.Intersections) {
                    var fDiff = City.Difference(Origin, inter.Position).Length;
                    var lDiff = City.Difference(inter.Position, Target).Length;

                    if (fDiff <= fBest) {
                        first = inter;
                        fBest = fDiff;
                    }

                    if (lDiff <= lBest) {
                        last = inter;
                        lBest = lDiff;
                    }
                }

                var path = AStar(City, first, last, NeighboursFunc, VectorFunc)
                    .Select(x => x.Position).ToArray();

                foreach (var node in path) yield return node;

                if (!path.Last().Equals(Target)) {
                    yield return Target;
                }
            }
        }

        private class MicroRoute : Route
        {
            private static IEnumerable<Tuple<Tile, float>> NeighboursFunc(City city, Tile tile)
            {
                for (int i = 0; i < 4; ++i) {
                    var face = (Face) (1 << i);
                    var othr = city.GetTile(new Vector2(tile.X, tile.Y) + face.GetNormal());

                    if (!tile.IsWallSolid(face)) {
                        yield return Tuple.Create(othr, 1f);
                    }
                }
            }

            private static Vector2 VectorFunc(Tile tile)
            {
                return new Vector2(tile.X + .5f, tile.Y + .5f);
            }

            public MicroRoute(City city, Vector2 origin, Vector2 target)
                : base(city, origin, target) { }

            public override IEnumerator<Vector2> GetEnumerator()
            {
                var path = AStar(City, City.GetTile(Origin), City.GetTile(Target), NeighboursFunc, VectorFunc)
                    .Select(x => new Vector2(x.X + .5f, x.Y + .5f)).ToArray();

                foreach (var node in path) yield return node;

                if (!path.Last().Equals(Target)) {
                    yield return Target;
                }
            }
        }

        public static Route Find(City city, Vector2 origin, Vector2 dest)
        {
            return new MicroRoute(city, origin, dest);
        }

        protected City City { get; private set; }
        public Vector2 Origin { get; private set; }
        public Vector2 Target { get; private set; }

        protected Route(City city, Vector2 origin, Vector2 target)
        {
            City = city;
            Origin = origin;
            Target = target;
        }

        public abstract IEnumerator<Vector2> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
