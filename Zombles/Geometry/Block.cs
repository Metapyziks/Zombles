using System;
using System.Collections.Generic;

using Zombles.Graphics;
using Zombles.Entities;

using OpenTKTK.Utils;
using System.Diagnostics;

namespace Zombles.Geometry
{
    public class Block : IEnumerable<Entity>
    {
        private Tile[,] _tiles;
        private List<Entity> _ents;

        private int _geomVertOffset;
        private int _baseFlatVertEnd;
        private int _baseWallVertEnd;
        private int _baseEdgeVertEnd;
        private int _topFlatVertEnd;
        private int _topWallVertEnd;
        private int _topEdgeVertEnd;

        public readonly World World;
        public readonly District District;

        public readonly int X;
        public readonly int Y;

        public readonly int Width;
        public readonly int Height;

        public Block(District district)
        {
            World = district.World;
            District = district;

            X = district.X;
            Y = district.Y;

            Width = district.Width;
            Height = district.Height;

            _tiles = new Tile[Width, Height];
            _ents = new List<Entity>();
        }

        public Tile this[int x, int y]
        {
            get { return _tiles[x - X, y - Y]; }
        }

        public bool Contains(int x, int y)
        {
            return x >= X && x < X + Width && y >= Y && y < Y + Height;
        }

        public void BuildTiles(TileBuilder[,] tiles)
        {
            lock (_tiles) {
                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    _tiles[x, y] = tiles[x, y].Create(X + x, Y + y);
                }
            }
        }

        internal void AddEntity(Entity ent)
        {
            _ents.Add(ent);
        }

        internal void RemoveEntity(Entity ent)
        {
            _ents.Remove(ent);
        }

        public void Think(double dt)
        {
            for (int i = _ents.Count - 1; i >= 0; --i) {
                _ents[i].Think(dt);
            }
        }

        public void PostThink()
        {
            for (int i = _ents.Count - 1; i >= 0; --i) {
                _ents[i].UpdateBlock();
            }
        }

        public int GetGeometryVertexCount()
        {
            _baseFlatVertEnd = 0;
            _baseWallVertEnd = 0;
            _baseEdgeVertEnd = 0;
            _topFlatVertEnd = 0;
            _topWallVertEnd = 0;
            _topEdgeVertEnd = 0;

            for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                _baseFlatVertEnd += _tiles[x, y].GetBaseFlatVertexCount();
                _baseWallVertEnd += _tiles[x, y].GetBaseWallVertexCount();
                _baseEdgeVertEnd += _tiles[x, y].GetBaseEdgeVertexCount();
                _topFlatVertEnd += _tiles[x, y].GetTopFlatVertexCount();
                _topWallVertEnd += _tiles[x, y].GetTopWallVertexCount();
                _topEdgeVertEnd += _tiles[x, y].GetTopEdgeVertexCount();
            }

            return _topEdgeVertEnd += _topWallVertEnd += _topFlatVertEnd
                += _baseEdgeVertEnd += _baseWallVertEnd += _baseFlatVertEnd;
        }

        public void GetGeometryVertices(float[] verts, ref int i)
        {
            _geomVertOffset = i / 3;

            lock (_tiles) {
                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    int prev = i; _tiles[x, y].GetBaseFlatVertices(verts, ref i);
                    Debug.Assert(i - prev == _tiles[x, y].GetBaseFlatVertexCount() * 3);
                }

                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    int prev = i; _tiles[x, y].GetBaseWallVertices(verts, ref i);
                    Debug.Assert(i - prev == _tiles[x, y].GetBaseWallVertexCount() * 3);
                }

                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    int prev = i; _tiles[x, y].GetBaseEdgeVertices(verts, ref i);
                    Debug.Assert(i - prev == _tiles[x, y].GetBaseEdgeVertexCount() * 3);
                }

                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    int prev = i; _tiles[x, y].GetTopFlatVertices(verts, ref i);
                    Debug.Assert(i - prev == _tiles[x, y].GetTopFlatVertexCount() * 3);
                }

                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    int prev = i; _tiles[x, y].GetTopWallVertices(verts, ref i);
                    Debug.Assert(i - prev == _tiles[x, y].GetTopWallVertexCount() * 3);
                }

                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    int prev = i; _tiles[x, y].GetTopEdgeVertices(verts, ref i);
                    Debug.Assert(i - prev == _tiles[x, y].GetTopEdgeVertexCount() * 3);
                }
            }
        }

        public void RenderGeometry(VertexBuffer vb, GeometryShader shader, bool baseOnly = false)
        {
            bool topDown = shader.IsTopDown;

            if (topDown) {
                vb.Render(_geomVertOffset, _baseFlatVertEnd);
                vb.Render(_geomVertOffset + _baseWallVertEnd, _baseEdgeVertEnd - _baseWallVertEnd);
            } else {
                vb.Render(_geomVertOffset, _baseWallVertEnd);
            }

            if (baseOnly) return;

            if (topDown) {
                vb.Render(_geomVertOffset + _baseEdgeVertEnd, _topFlatVertEnd - _baseEdgeVertEnd);
                vb.Render(_geomVertOffset + _topWallVertEnd, _topEdgeVertEnd - _topWallVertEnd);
            } else {
                vb.Render(_geomVertOffset + _baseEdgeVertEnd, _topWallVertEnd - _baseEdgeVertEnd);
            }
        }

        public void RenderEntities(FlatEntityShader shader)
        {
            foreach (Entity ent in _ents) {
                if (ent.HasComponent<Render2D>()) {
                    ent.GetComponent<Render2D>().OnRender(shader);
                }
            }
        }

        public void RenderEntities(ModelEntityShader shader)
        {
            foreach (Entity ent in _ents) {
                if (ent.HasComponent<Render3D>()) {
                    ent.GetComponent<Render3D>().OnRender(shader);
                }
            }
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return _ents.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _ents.GetEnumerator();
        }
    }
}
