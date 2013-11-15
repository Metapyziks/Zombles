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

            public void CalculateHeuristic(World world, Vector2 target)
            {
                Heuristic = world.Difference(Pos, target).Length;
            }
        }

        private static IEnumerable<T> AStar<T>(World world, T origin, T target,
            Func<World, T, IEnumerable<Tuple<T, float>>> adjFunc, Func<T, Vector2> vecFunc)
        {
            var open = new List<NodeInfo<T>>();
            var clsd = new HashSet<NodeInfo<T>>();

            var targPos = vecFunc(target);

            var first = new NodeInfo<T>(origin, vecFunc(origin));
            first.CalculateHeuristic(world, targPos);

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

                foreach (var adj in adjFunc(world, cur.Node)) {
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
                        node.CalculateHeuristic(world, targPos);
                    }

                    open.Add(node);
                }
            }

            return null;
        }

        private class MacroRoute : Route
        {
            private static IEnumerable<Tuple<Intersection, float>> NeighboursFunc(World world, Intersection inter)
            {
                return inter.Edges.Select(x => Tuple.Create(x.Key, x.Value.Length));
            }

            private static Vector2 VectorFunc(Intersection inter)
            {
                return inter.Position;
            }

            public MacroRoute(World world, Vector2 origin, Vector2 target)
                : base(world, origin, target) { }

            public override IEnumerator<Vector2> GetEnumerator()
            {
                Intersection first = null;
                Intersection last = null;

                float fBest = float.MaxValue;
                float lBest = float.MaxValue;

                var firstBlock = World.GetBlock(Origin);
                var lastBlock = World.GetBlock(Target);

                if (firstBlock == lastBlock) {
                    yield return Target;
                    yield break;
                }

                foreach (var inter in World.GetIntersections(firstBlock)) {
                    var fDiff = World.Difference(Target, inter.Position).Length + World.Difference(inter.Position, Origin).Length;
                    if (fDiff <= fBest) {
                        first = inter;
                        fBest = fDiff;
                    }
                }

                foreach (var inter in World.GetIntersections(lastBlock)) {
                    var lDiff = World.Difference(Target, inter.Position).Length + World.Difference(inter.Position, Origin).Length;
                    if (lDiff <= lBest) {
                        last = inter;
                        lBest = lDiff;
                    }
                }

                if (first == last) {
                    yield return last.Position;
                    yield return Target;
                    yield break;
                }

                var path = AStar(World, first, last, NeighboursFunc, VectorFunc)
                    .Select(x => x.Position).ToArray();

                foreach (var node in path) yield return node;

                if (!path.Last().Equals(Target)) {
                    yield return Target;
                }
            }
        }

        private class MicroRoute : Route
        {
            private IEnumerable<Block> _blocks;

            public MicroRoute(World world, Vector2 origin, Vector2 target, IEnumerable<Block> blocks = null)
                : base(world, origin, target)
            {
                _blocks = blocks;
            }

            private IEnumerable<Tuple<Tile, float>> NeighboursFunc(World world, Tile tile)
            {
                for (int i = 0; i < 4; ++i) {
                    var face = (Face) (1 << i);
                    var othr = world.GetTile(new Vector2(tile.X, tile.Y) + face.GetNormal());

                    if (_blocks != null && !_blocks.Any(x => x.Contains(othr.X, othr.Y))) {
                        continue;
                    }

                    if (!tile.IsWallSolid(face)) {
                        yield return Tuple.Create(othr, 1f);
                    }
                }
            }

            private Vector2 VectorFunc(Tile tile)
            {
                return new Vector2(tile.X + .5f, tile.Y + .5f);
            }

            public override IEnumerator<Vector2> GetEnumerator()
            {
                var nodes = AStar(World, World.GetTile(Origin), World.GetTile(Target), NeighboursFunc, VectorFunc);

                if (nodes == null) yield break;

                var path = nodes.Select(x => new Vector2(x.X + .5f, x.Y + .5f)).ToArray();

                foreach (var node in path) yield return node;

                if (!path.Last().Equals(Target)) {
                    yield return Target;
                }
            }
        }

        private class CombinedRoute : Route
        {
            private class CombinedRouteEnumerator : IEnumerator<Vector2>
            {
                private World _city;
                private Vector2 _origin;
                private IEnumerable<Vector2> _macro;

                private Vector2 _prevMacro;
                private IEnumerator<Vector2> _macroIter;
                private IEnumerator<Vector2> _microIter;

                public CombinedRouteEnumerator(World world, Vector2 origin, IEnumerable<Vector2> macro)
                {
                    _city = world;
                    _origin = origin;
                    _macro = macro;

                    Reset();
                }

                public Vector2 Current
                {
                    get { return _microIter.Current; }
                }

                public void Dispose()
                {
                    if (_macroIter != null) _macroIter.Dispose();
                    if (_microIter != null) _microIter.Dispose();
                }

                object System.Collections.IEnumerator.Current
                {
                    get { return _microIter.Current; }
                }

                public bool MoveNext()
                {
                    while (_microIter == null || !_microIter.MoveNext()) {
                        if (_microIter != null) _prevMacro = _macroIter.Current;
                        if (!_macroIter.MoveNext()) return false;

                        var micro = new MicroRoute(_city, _prevMacro, _macroIter.Current);
                        _microIter = micro.GetEnumerator();
                    }

                    return true;
                }

                public void Reset()
                {
                    _prevMacro = _origin;

                    _macroIter = _macro.GetEnumerator();
                    _microIter = null;
                }
            }

            public CombinedRoute(World world, Vector2 origin, Vector2 target)
                : base(world, origin, target) { }

            public override IEnumerator<Vector2> GetEnumerator()
            {
                var macro = new MacroRoute(World, Origin, Target);
                return new CombinedRouteEnumerator(World, Origin, macro);
            }
        }

        public static Route Find(World world, Vector2 origin, Vector2 dest)
        {
            return new CombinedRoute(world, origin, dest);
        }

        public static Route FindRefined(World world, Vector2 origin, Vector2 dest)
        {
            return new MicroRoute(world, origin, dest);
        }

        protected World World { get; private set; }
        public Vector2 Origin { get; private set; }
        public Vector2 Target { get; private set; }

        public float EstimateLength
        {
            get
            {
                float length = 0f;
                var prev = Origin;

                var trace = new Trace(World) {
                    Origin = Origin,
                    HitEntities = false,
                    HitGeometry = true,
                    HullSize = new Vector2(0.5f, 0.5f)
                };

                foreach (var node in this) {
                    trace.Target = node;
                    if (node.Equals(Target)) {
                        if (trace.GetResult().Hit) {
                            length += World.Difference(trace.Origin, prev).Length;
                            trace.Origin = prev;
                        }
                        length += World.Difference(trace.Origin, node).Length;
                        break;
                    } else if (trace.GetResult().Hit) {
                        length += World.Difference(trace.Origin, prev).Length;
                        trace.Origin = prev;
                    }
                    prev = node;
                }

                return length;
            }
        }

        protected Route(World world, Vector2 origin, Vector2 target)
        {
            World = world;
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
