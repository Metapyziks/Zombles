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
        private VertexBuffer _geomVertexBuffer;
        private VertexBuffer _pathVertexBuffer;
        private AlphaTexture2D _bloodMap;

        private bool _setPathVB;

        public District RootDistrict { get; private set; }

        public int Width { get { return RootDistrict.Width; } }
        public int Height { get { return RootDistrict.Height; } }

        public int Depth { get { return RootDistrict.Depth; } }

        public City(int width, int height)
        {
            RootDistrict = new District(this, 0, 0, width, height);
            _geomVertexBuffer = new VertexBuffer(3);
            _pathVertexBuffer = new VertexBuffer(2);
            _bloodMap = new AlphaTexture2D(width * 2, height * 2);

            _setPathVB = false;
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
                float dist = i * 0.125f;

                float x = pos.X + (Tools.Random.NextSingle() * 2.0f - 1.0f) * dist;
                float y = pos.Y + (Tools.Random.NextSingle() * 2.0f - 1.0f) * dist;

                TraceResult res = Trace.Quick(this, pos, new Vector2(x, y), false, true, new Vector2(0.25f, 0.25f));

                int ix = (int) (res.End.X * 2.0f);
                int iy = (int) (res.End.Y * 2.0f);

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

        public void UpdateGeometryVertexBuffer()
        {
            int count = RootDistrict.GetGeometryVertexCount();
            float[] verts = new float[count * _geomVertexBuffer.Stride];
            int i = 0;
            RootDistrict.GetGeometryVertices(verts, ref i);
            _geomVertexBuffer.SetData(verts);
        }

        public void UpdatePathVertexBuffer()
        {
            int count = RootDistrict.GetPathVertexCount();
            float[] verts = new float[count * _pathVertexBuffer.Stride];
            int i = 0;
            RootDistrict.GetPathVertices(verts, ref i);
            _pathVertexBuffer.SetData(verts);
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
