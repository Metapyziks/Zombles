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
    public class City : IEnumerable<Block>, IDisposable
    {
        private class Intersection
        {
            private Dictionary<Intersection, float> _edges;
            
            public Vector2 Position { get; private set; }
            public int ID { get; private set; }

            public float X { get { return Position.X; } }
            public float Y { get { return Position.Y; } }

            public IEnumerable<KeyValuePair<Intersection, float>> Edges
            {
                get { return _edges; }
            }

            public Intersection(Vector2 pos, int id)
            {
                Position = pos;
                ID = id;

                _edges = new Dictionary<Intersection, float>();
            }

            public void Connect(Intersection other)
            {
                if (!_edges.ContainsKey(other)) {
                    _edges.Add(other, (other.Position - Position).Length);
                }
            }

            public override bool Equals(object obj)
            {
                return obj is Intersection && ((Intersection) obj).Position.Equals(Position);
            }

            public override int GetHashCode()
            {
                return Position.GetHashCode();
            }

            public override string ToString()
            {
                return Position.ToString();
            }
        }

        private const int BloodResolution = 2;

        private VertexBuffer _geomVertexBuffer;
        private AlphaTexture2D _bloodMap;

        private Dictionary<Block, Intersection[]> _intersections;

        public District RootDistrict { get; private set; }

        public int Width { get { return RootDistrict.Width; } }
        public int Height { get { return RootDistrict.Height; } }

        public int Depth { get { return RootDistrict.Depth; } }

        public IEnumerable<Entity> Entities { get { return this.SelectMany(x => x); } }

        public City(int width, int height)
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
            return RootDistrict.GetBlock(pos.X, pos.Y);
        }

        public Block GetBlock(float x, float y)
        {
            return RootDistrict.GetBlock(x, y);
        }

        public Tile GetTile(Vector2 pos)
        {
            pos = Wrap(pos);

            int ix = (int) pos.X;
            int iy = (int) pos.Y;

            return GetBlock(pos)[ix, iy];
        }

        public void FindBlockIntersections()
        {
            _intersections = new Dictionary<Block, Intersection[]>();

            var all = RootDistrict.SelectMany(block =>
                new Vector2[] {
                    new Vector2(block.District.X, block.District.Y),
                    new Vector2(block.District.X + block.District.Width, block.District.Y),
                    new Vector2(block.District.X + block.District.Width, block.District.Y + block.District.Height),
                    new Vector2(block.District.X, block.District.Y + block.District.Height)
                }
            ).Distinct().Select((x, i) => new Intersection(x, i));

            foreach (var block in RootDistrict) {
                var l = block.District.X;
                var r = l + block.District.Width;
                var t = block.District.Y;
                var b = t + block.District.Height;

                var ints = all.Where(y => y.X >= l && y.X <= r && y.Y >= t && y.Y <= b).ToArray();

                foreach (var first in ints) {
                    foreach (var secnd in ints) {
                        if (first.Equals(secnd)) continue;

                        if (first.X == secnd.X || first.Y == secnd.Y) {
                            first.Connect(secnd);
                            secnd.Connect(first);
                        }
                    }
                }

                _intersections.Add(block, ints);
            }
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
            RootDistrict.RenderGeometry(_geomVertexBuffer, shader, baseOnly);
            _geomVertexBuffer.End();
        }

        public void RenderEntities(FlatEntityShader shader)
        {
            RootDistrict.RenderEntities(shader);
        }

        public void RenderIntersectionNetwork(DebugTraceShader shader)
        {
            foreach (var inter in _intersections.Values.SelectMany(x => x)) {
                foreach (var other in inter.Edges.Where(x => x.Key.ID < inter.ID).Select(x => x.Key)) {
                    shader.Render(inter.X, inter.Y, other.X, other.Y);
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
