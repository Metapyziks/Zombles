using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Graphics;
using Zombles.Entities;

using OpenTKTK.Utils;
using OpenTKTK.Textures;

namespace Zombles.Geometry
{
    public class World : IEnumerable<Block>, IDisposable
    {
        private const int BloodResolution = 2;

        private VertexBuffer _geomVertexBuffer;
        private AlphaTexture2D _bloodMap;

        private Dictionary<Block, Intersection[]> _intersections;

        public District RootDistrict { get; private set; }

        public int Width { get { return RootDistrict.Width; } }
        public int Height { get { return RootDistrict.Height; } }

        public int Depth { get { return RootDistrict.Depth; } }

        public IEnumerable<Entity> Entities { get { return this.SelectMany(x => x); } }

        public World(int width, int height)
        {
            RootDistrict = new District(this, 0, 0, width, height);

            _geomVertexBuffer = new VertexBuffer(3);
            _bloodMap = new AlphaTexture2D(width * BloodResolution, height * BloodResolution);
        }

        public Vector2 Difference(Vector2 a, Vector2 b)
        {
            Vector2 diff = b - a;

            if (diff.X >= Width >> 1)
                diff.X -= Width;
            else if (diff.X < -(Width >> 1))
                diff.X += Width;

            if (diff.Y >= Height >> 1)
                diff.Y -= Height;
            else if (diff.Y < -(Height >> 1))
                diff.Y += Height;

            return diff;
        }

        public Vector2 Wrap(Vector2 pos)
        {
            pos.X -= (int) Math.Floor(pos.X / Width) * Width;
            pos.Y -= (int) Math.Floor(pos.Y / Height) * Height;

            return pos;
        }

        public void SplashBlood(Vector2 pos, float force)
        {
            if (force <= 0.0f)
                return;

            int count = (int) (force * 4.0f);

            for (int i = 0; i < count; ++i) {
                float dist = (i + 1) * 0.125f;

                float x = pos.X + (Tools.Random.NextSingle() * 2.0f - 1.0f) * dist;
                float y = pos.Y + (Tools.Random.NextSingle() * 2.0f - 1.0f) * dist;

                TraceResult res = Trace.Quick(this, pos, new Vector2(x, y), false, true, new Vector2(0.25f, 0.25f));
                
                int ix = (int) (res.End.X * BloodResolution);
                int iy = (int) (res.End.Y * BloodResolution);

                _bloodMap[ix, iy] = Math.Min(1f, _bloodMap[ix, iy]
                    + 1f / 32f + Tools.Random.NextSingle() * 1f / 6f);
            }
        }

        public Block GetBlock(Vector2 pos)
        {
            pos = Wrap(pos);

            return RootDistrict.GetBlock(pos.X, pos.Y);
        }

        public Block GetBlock(float x, float y)
        {
            return GetBlock(new Vector2(x, y));
        }

        public Tile GetTile(Vector2 pos)
        {
            pos = Wrap(pos);

            int ix = (int) pos.X;
            int iy = (int) pos.Y;

            return GetBlock(pos)[ix, iy];
        }

        public IEnumerable<Intersection> GetIntersections()
        {
            return _intersections.Values.SelectMany(x => x);
        }

        public IEnumerable<Intersection> GetIntersections(Block block)
        {
            return _intersections[block];
        }

        public void FindBlockIntersections()
        {
            _intersections = new Dictionary<Block, Intersection[]>();

            var all = RootDistrict.SelectMany(block =>
                new Vector2[] {
                    Wrap(new Vector2(block.District.X, block.District.Y)),
                    Wrap(new Vector2(block.District.X + block.District.Width, block.District.Y)),
                    Wrap(new Vector2(block.District.X + block.District.Width, block.District.Y + block.District.Height)),
                    Wrap(new Vector2(block.District.X, block.District.Y + block.District.Height))
                }
            ).Distinct().Select((x, i) => new Intersection(x, i)).ToArray();

            foreach (var block in RootDistrict) {
                var tl = new Vector2(block.District.X, block.District.Y);
                var br = tl + new Vector2(block.District.Width, block.District.Height);
                var brw = Wrap(br);

                var ints = all.Where(y => ((y.X >= tl.X && y.X <= br.X) || y.X == brw.X)
                    && ((y.Y >= tl.Y && y.Y <= br.Y) || y.Y == brw.Y)).ToArray();

                foreach (var first in ints) {
                    Action<Tuple<Intersection, Vector2>> join = secnd => {
                        if (secnd == null) return;
                        first.Connect(secnd.Item1, secnd.Item2);
                        secnd.Item1.Connect(first, -secnd.Item2);
                    };

                    Func<IEnumerable<Intersection>, IEnumerable<Tuple<Intersection, Vector2>>> filter =
                        list => list
                            .Select(secnd => Tuple.Create(secnd, Difference(first.Position, secnd.Position)))
                            .OrderBy(x => x.Item2.LengthSquared);

                    if (first.X == tl.X || first.X == brw.X) {
                        var horz = filter(ints.Where(secnd => secnd != first && first.X == secnd.X));
                        join(horz.FirstOrDefault(x => x.Item2.Y < 0));
                        join(horz.FirstOrDefault(x => x.Item2.Y > 0));
                    }

                    if (first.Y == tl.Y || first.Y == brw.Y) {
                        var vert = filter(ints.Where(secnd => secnd != first && first.Y == secnd.Y));
                        join(vert.FirstOrDefault(x => x.Item2.X < 0));
                        join(vert.FirstOrDefault(x => x.Item2.X > 0));
                    }
                }

                _intersections.Add(block, ints);
            }
        }

        public bool IsPositionNavigable(Vector2 pos)
        {
            if (GetTile(pos).FloorHeight > 0) return false;

            var inter = GetIntersections(GetBlock(pos))
                .OrderBy(x => (pos - x.Position).LengthSquared).First();

            return Route.FindRefined(this, pos, inter.Position).Count() > 0;
        }

        public void UpdateGeometryVertexBuffer()
        {
            int count = RootDistrict.GetGeometryVertexCount();
            float[] verts = new float[count * _geomVertexBuffer.Stride];
            int i = 0;
            RootDistrict.GetGeometryVertices(verts, ref i);
            _geomVertexBuffer.SetData(verts);
        }

        public void Think(double dt)
        {
            RootDistrict.Think(dt);
            RootDistrict.PostThink();
        }

        public void RenderGeometry(GeometryShader shader, bool baseOnly = false)
        {
            shader.SetTexture("bloodmap", _bloodMap);
            _geomVertexBuffer.Begin(shader);
            foreach (var block in RootDistrict.GetVisibleBlocks(shader.Camera)) {
                block.RenderGeometry(_geomVertexBuffer, shader, baseOnly);
            }
            _geomVertexBuffer.End();
        }

        public void RenderEntities(FlatEntityShader shader)
        {
            foreach (var block in RootDistrict.GetVisibleBlocks(shader.Camera)) {
                block.RenderEntities(shader);
            }
        }

        public void RenderIntersectionNetwork(DebugTraceShader shader)
        {
            var denom = _intersections.Count / 4f;
            foreach (var block in RootDistrict.GetVisibleBlocks(shader.Camera)) {
                foreach (var inter in _intersections[block]) {
                    foreach (var edge in inter.Edges) {
                        if (edge.Key.ID < inter.ID) {
                            shader.Render(inter.X, inter.ID / denom, inter.Y,
                                inter.X + edge.Value.X, edge.Key.ID / denom, inter.Y + edge.Value.Y);
                        }
                    }
                }
            }
        }

        public IEnumerator<Block> GetEnumerator()
        {
            return new DistrictEnumerator(RootDistrict);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            _geomVertexBuffer.Dispose();
        }
    }
}
