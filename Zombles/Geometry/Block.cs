using System;
using System.Collections.Generic;

using Zombles.Graphics;
using Zombles.Entities;

using OpenTKTK.Utils;

namespace Zombles.Geometry
{
    public class Block : IEnumerable<Entity>
    {
        private Tile[,] _tiles;
        private List<Entity> _ents;

        private int _baseGeomVertCount;
        private int _topGeomVertCount;
        private int _geomVertOffset;

        private int _pathVertCount;
        private int _pathVertOffset;

        public readonly City City;
        public readonly District District;

        public readonly int X;
        public readonly int Y;

        public readonly int Width;
        public readonly int Height;

        public Block(District district)
        {
            City = district.City;
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
            get
            {
                return _tiles[x - X, y - Y];
            }
        }

        public void BuildTiles(TileBuilder[,] tiles)
        {
            lock (_tiles)
                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y)
                        _tiles[x, y] = tiles[x, y].Create(X + x, Y + y);
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
            for (int i = _ents.Count - 1; i >= 0; --i)
                _ents[i].Think(dt);
        }

        public void PostThink()
        {
            for (int i = _ents.Count - 1; i >= 0; --i)
                _ents[i].UpdateBlock();
        }

        public int GetGeometryVertexCount()
        {
            _baseGeomVertCount = 0;
            _topGeomVertCount = 0;

            for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    _baseGeomVertCount += _tiles[x, y].GetBaseVertexCount();
                    _topGeomVertCount += _tiles[x, y].GetTopVertexCount();
                }

            return _baseGeomVertCount + _topGeomVertCount;
        }

        public void GetGeometryVertices(float[] verts, ref int i)
        {
            _geomVertOffset = i / 3;

            lock (_tiles) {
                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y)
                        _tiles[x, y].GetBaseVertices(verts, ref i);

                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y)
                        _tiles[x, y].GetTopVertices(verts, ref i);
            }
        }

        public void RenderGeometry(VertexBuffer vb, GeometryShader shader, bool baseOnly = false)
        {
            vb.Render(_geomVertOffset, (baseOnly ? _baseGeomVertCount : _baseGeomVertCount + _topGeomVertCount));
        }

        public void RenderEntities(FlatEntityShader shader)
        {
            foreach (Entity ent in _ents)
                if (ent.HasComponent<Render2D>())
                    ent.GetComponent<Render2D>().OnRender(shader);
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
