using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Zombles.Graphics;
using Zombles.Entities;

using OpenTKTK.Utils;
using OpenTK;

namespace Zombles.Geometry
{
    public class Block : IEnumerable<Entity>
    {
        private Tile[,] _tiles;
        private List<Entity> _ents;

        private bool _enclosedInvalid;

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

        public bool Enclosed { get; private set; }

        public bool HasInterior { get; private set; }

        public IEnumerable<Intersection> Intersections
        {
            get { return World.GetIntersections(this); }
        }
        
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

            _enclosedInvalid = true;
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
                HasInterior = false;
                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    _tiles[x, y] = tiles[x, y].Create(X + x, Y + y);
                    HasInterior = HasInterior || _tiles[x, y].IsInterior;
                }
            }
        }

        internal void FindTileNeighbours()
        {
            foreach (var tile in _tiles) {
                tile.FindNeighbours(World);
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

        public void InvalidateEnclosedness()
        {
            _enclosedInvalid = true;
        }

        private void TestEnclosedness()
        {
            _enclosedInvalid = false;

            var tiles = new List<Tile>(_tiles.Length);

            Enclosed = true;

            foreach (var tile in _tiles) {
                if (tile.IsInterior && !tile.IsSolid) {
                    tiles.Add(tile);
                }
            }

            tiles = tiles.OrderBy(x => Tools.Random.Next()).Take(16).ToList();

            var dests = new[] {
                new Vector2(X, Y + Height / 2),
                new Vector2(X + Width, Y + Height / 2),
                new Vector2(X + Width / 2, Y),
                new Vector2(X + Width / 2, Y + Height)
            };

            Enclosed = !tiles
                .Select(x => new Vector2(x.X + 0.5f, x.Y + 0.5f))
                .Any(x => Route.FindRefined(World, x, dests
                    .OrderBy(y => World.Difference(x, y).LengthSquared)
                    .First()).GetEnumerator().MoveNext());
        }

        public void PostThink()
        {
            for (int i = _ents.Count - 1; i >= 0; --i) {
                _ents[i].UpdateBlock();
            }

            if (_enclosedInvalid) {
                TestEnclosedness();
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
                    Trace.Assert(i - prev == _tiles[x, y].GetBaseFlatVertexCount() * 3);
                }

                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    int prev = i; _tiles[x, y].GetBaseWallVertices(verts, ref i);
                    Trace.Assert(i - prev == _tiles[x, y].GetBaseWallVertexCount() * 3);
                }

                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    int prev = i; _tiles[x, y].GetBaseEdgeVertices(verts, ref i);
                    Trace.Assert(i - prev == _tiles[x, y].GetBaseEdgeVertexCount() * 3);
                }

                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    int prev = i; _tiles[x, y].GetTopFlatVertices(verts, ref i);
                    Trace.Assert(i - prev == _tiles[x, y].GetTopFlatVertexCount() * 3);
                }

                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    int prev = i; _tiles[x, y].GetTopWallVertices(verts, ref i);
                    Trace.Assert(i - prev == _tiles[x, y].GetTopWallVertexCount() * 3);
                }

                for (int x = 0; x < Width; ++x) for (int y = 0; y < Height; ++y) {
                    int prev = i; _tiles[x, y].GetTopEdgeVertices(verts, ref i);
                    Trace.Assert(i - prev == _tiles[x, y].GetTopEdgeVertexCount() * 3);
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
                ent.Render(shader);
            }
        }

        public void RenderEntities(ModelEntityShader shader)
        {
            foreach (Entity ent in _ents) {
                ent.Render(shader);
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
