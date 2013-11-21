using System;

using OpenTK;
using System.Linq;
using System.Collections.Generic;
using Zombles.Entities;

namespace Zombles.Geometry
{
    public class Tile
    {
        private HashSet<Entity> _staticEnts;
        private Tile[] _neighbours;

        public readonly int X;
        public readonly int Y;

        public readonly byte WallHeight;
        public readonly byte FloorHeight;
        public readonly byte RoofHeight;
        public readonly Face RoofSlant;
        public readonly ushort FloorTileIndex;
        public readonly ushort RoofTileIndex;

        public readonly ushort[,] WallTileIndices;

        public bool IsSolid
        {
            get { return FloorHeight > 0 || _staticEnts.Count > 0; }
        }

        public bool IsInterior
        {
            get { return RoofHeight > FloorHeight && RoofTileIndex != 0xffff; }
        }

        public Tile(int x, int y, TileBuilder builder)
        {
            _staticEnts = new HashSet<Entity>();
            _neighbours = new Tile[4];

            X = x;
            Y = y;

            WallHeight = builder.WallHeight;
            FloorHeight = builder.FloorHeight;
            RoofHeight = builder.RoofHeight;

            RoofSlant = builder.RoofSlant;

            FloorTileIndex = builder.FloorTileIndex;
            RoofTileIndex = builder.RoofTileIndex;
            WallTileIndices = builder.GetWallTileIndices();
        }

        internal void FindNeighbours(World world)
        {
            for (int i = 0; i < 4; ++i) {
                var face = (Face) (1 << i);
                _neighbours[i] = world.GetTile(new Vector2(X + 0.5f, Y + 0.5f) + face.GetNormal());
            }
        }
        
        internal void AddStaticEntity(Entity ent)
        {
            _staticEnts.Add(ent);
        }

        internal void RemoveStaticEntity(Entity ent)
        {
            _staticEnts.Remove(ent);
        }

        public bool IsWallSolid(Face face)
        {
            if (_neighbours[face.GetIndex()].IsSolid) return true;
            if (WallHeight == 0) return false;

            return WallTileIndices[face.GetIndex(), 0] != 0xffff;
        }

        public int GetBaseFlatVertexCount()
        {
            int count = 0;

            if (FloorHeight <= 2 && FloorTileIndex != 0xffff) count += 4;
            if (RoofHeight > FloorHeight && RoofHeight < 2 && RoofTileIndex != 0xffff) count += 4;

            return count;
        }

        public int GetBaseWallVertexCount()
        {
            int count = 0;

            for (int i = FloorHeight; i < Math.Min(WallHeight, (byte) 2); ++i) {
                for (int j = 0; j < 4; ++j) {
                    if (WallTileIndices[j, i] != 0xffff) count += 4;
                }
            }

            return count;
        }

        public int GetBaseEdgeVertexCount()
        {
            int count = 0;

            for (int j = 0; j < 4; ++j) {
                for (int i = Math.Min(WallHeight, (byte) 2) - 1; i >= FloorHeight; --i) {
                    if (WallTileIndices[j, i] != 0xffff) {
                        count += 4;
                        //break;
                    }
                }
            }

            return count;
        }

        public int GetTopFlatVertexCount()
        {
            int count = 0;

            if (FloorHeight > 2 && FloorTileIndex != 0xffff) count += 4;
            if (RoofHeight > FloorHeight && RoofHeight >= 2 && RoofTileIndex != 0xffff) count += 4;

            return count;
        }

        public int GetTopWallVertexCount()
        {
            int count = 0;

            for (int i = Math.Max(FloorHeight, (byte) 2); i < WallHeight; ++i) {
                for (int j = 0; j < 4; ++j) {
                    if (WallTileIndices[j, i] != 0xffff) count += 4;
                }
            }

            return count;
        }

        public int GetTopEdgeVertexCount()
        {
            int count = 0;

            for (int j = 0; j < 4; ++j) {
                for (int i = WallHeight - 1; i >= Math.Max(FloorHeight, (byte) 2); --i) {
                    if (WallTileIndices[j, i] != 0xffff) {
                        count += 4;
                        //break;
                    }
                }
            }

            return count;
        }

        public void GetBaseFlatVertices(float[] verts, ref int offset)
        {
            if (FloorHeight <= 2) {
                GetFlatVertices(FloorHeight, Face.None, FloorTileIndex, verts, ref offset);
            }

            if (RoofHeight > FloorHeight && RoofHeight < 2) {
                GetFlatVertices(RoofHeight, RoofSlant, RoofTileIndex, verts, ref offset);
            }
        }

        public void GetBaseWallVertices(float[] verts, ref int offset)
        {
            for (int i = FloorHeight; i < Math.Min(WallHeight, (byte) 2); ++i) {
                for (int j = 0; j < 4; ++j) {
                    GetWallVertices((Face) (1 << j), i, WallTileIndices[j, i], verts, ref offset);
                }
            }
        }

        public void GetBaseEdgeVertices(float[] verts, ref int offset)
        {
            for (int j = 0; j < 4; ++j) {
                for (int i = Math.Min(WallHeight, (byte) 2) - 1; i >= FloorHeight; --i) {
                    if (WallTileIndices[j, i] != 0xffff) {
                        GetEdgeVertices((Face) (1 << j), i, WallTileIndices[j, i], verts, ref offset);
                        //break;
                    }
                }
            }
        }

        public void GetTopFlatVertices(float[] verts, ref int offset)
        {
            if (FloorHeight > 2) {
                GetFlatVertices(FloorHeight, Face.None, FloorTileIndex, verts, ref offset);
            }

            if (RoofHeight > FloorHeight && RoofHeight >= 2) {
                GetFlatVertices(RoofHeight, RoofSlant, RoofTileIndex, verts, ref offset);
            }
        }

        public void GetTopWallVertices(float[] verts, ref int offset)
        {
            for (int i = Math.Max(FloorHeight, (byte) 2); i < WallHeight; ++i) {
                for (int j = 0; j < 4; ++j) {
                    GetWallVertices((Face) (1 << j), i, WallTileIndices[j, i], verts, ref offset);
                }
            }
        }

        public void GetTopEdgeVertices(float[] verts, ref int offset)
        {
            for (int j = 0; j < 4; ++j) {
                for (int i = WallHeight - 1; i >= Math.Max(FloorHeight, (byte) 2); --i) {
                    if (WallTileIndices[j, i] != 0xffff) {
                        GetEdgeVertices((Face) (1 << j), i, WallTileIndices[j, i], verts, ref offset);
                        //break;
                    }
                }
            }
        }

        private void GetFlatVertices(int level, Face slant, ushort tile, float[] verts, ref int i)
        {
            if (tile == 0xffff) return;

            int tt = (tile << (4 + 4)) | ((level & 0xf) << 4)
                | (level == FloorHeight && RoofHeight > FloorHeight && RoofTileIndex != 0xffff ? 8 : 0);

            int sn = (slant & Face.North) != 0 ? 0x10 : 0;
            int ss = (slant & Face.South) != 0 ? 0x10 : 0;
            int se = (slant & Face.East) != 0 ? 0x10 : 0;
            int sw = (slant & Face.West) != 0 ? 0x10 : 0;

            Vector2 tl = new Vector2(X, Y) * 8f;
            Vector2 br = new Vector2(X + 1f, Y + 1f) * 8f;

            verts[i++] = tl.X; verts[i++] = tl.Y; verts[i++] = (tt + (sn | sw)) | 0x0;
            verts[i++] = br.X; verts[i++] = tl.Y; verts[i++] = (tt + (sn | se)) | 0x1;
            verts[i++] = br.X; verts[i++] = br.Y; verts[i++] = (tt + (ss | se)) | 0x5;
            verts[i++] = tl.X; verts[i++] = br.Y; verts[i++] = (tt + (ss | sw)) | 0x4;
        }

        private void GetWallVertices(Face face, int level, ushort tile, float[] verts, ref int i)
        {
            if (tile == 0xffff) return;

            Vector2 tl, br;

            switch (face) {
                case Face.West:
                    tl = new Vector2(X + 0.0f, Y + 1.0f);
                    br = new Vector2(X + 0.0f, Y + 0.0f);
                    break;
                case Face.North:
                    tl = new Vector2(X + 0.0f, Y + 0.0f);
                    br = new Vector2(X + 1.0f, Y + 0.0f);
                    break;
                case Face.East:
                    tl = new Vector2(X + 1.0f, Y + 0.0f);
                    br = new Vector2(X + 1.0f, Y + 1.0f);
                    break;
                case Face.South:
                    tl = new Vector2(X + 1.0f, Y + 1.0f);
                    br = new Vector2(X + 0.0f, Y + 1.0f);
                    break;
                default:
                    return;
            }

            int texData = tile << (4 + 4);
            int shade = (((int) face) & 0xa) != 0 ? 8 : 0;
            int tt = texData | (((level + 1) & 0xf) << 4) | shade;
            int bt = texData | ((level & 0xf) << 4) | shade;

            int ol = (level & 0x1) << 1;

            int xFace = (((int) face & 0xa) != 0 ? 0x1000 : 0);
            int yFace = (((int) face & 0xc) != 0 ? 0x1000 : 0);

            verts[i++] = ((int) Math.Round(tl.X * 8f) & 0xfff) | xFace;
            verts[i++] = ((int) Math.Round(tl.Y * 8f) & 0xfff) | yFace;
            verts[i++] = tt | (0x0 + ol);
            verts[i++] = ((int) Math.Round(br.X * 8f) & 0xfff) | xFace;
            verts[i++] = ((int) Math.Round(br.Y * 8f) & 0xfff) | yFace;
            verts[i++] = tt | (0x1 + ol);
            verts[i++] = ((int) Math.Round(br.X * 8f) & 0xfff) | xFace;
            verts[i++] = ((int) Math.Round(br.Y * 8f) & 0xfff) | yFace;
            verts[i++] = bt | (0x3 + ol);
            verts[i++] = ((int) Math.Round(tl.X * 8f) & 0xfff) | xFace;
            verts[i++] = ((int) Math.Round(tl.Y * 8f) & 0xfff) | yFace;
            verts[i++] = bt | (0x2 + ol);
        }

        private void GetEdgeVertices(Face face, int level, ushort tile, float[] verts, ref int i)
        {
            Vector2 tl, br; int nx, ny;

            float ed = 1f / 8f;

            switch (face) {
                case Face.West:
                    tl = new Vector2(X + 0f, Y + 0f);
                    br = new Vector2(X + ed, Y + 1f);
                    nx = 0; ny = 1;
                    break;
                case Face.North:
                    tl = new Vector2(X + 0f, Y + 0f);
                    br = new Vector2(X + 1f, Y + ed);
                    nx = 1; ny = 0;
                    break;
                case Face.East:
                    tl = new Vector2(X + 1f - ed, Y + 0f);
                    br = new Vector2(X + 1f, Y + 1f);
                    nx = 0; ny = 1;
                    break;
                case Face.South:
                    tl = new Vector2(X + 0f, Y + 1f - ed);
                    br = new Vector2(X + 1f, Y + 1f);
                    nx = 1; ny = 0;
                    break;
                default:
                    return;
            }

            int texData = tile << (4 + 4);
            int shade = (((int) face) & 0xa) != 0 ? 8 : 0;
            int tt = texData | (((level + 1) & 0xf) << 4) | shade;

            int ol = (level & 0x1) << 1;

            int xFace = (((int) face & 0xa) != 0 ? 0x1000 : 0);
            int yFace = (((int) face & 0xc) != 0 ? 0x1000 : 0);

            verts[i++] = ((int) Math.Round(tl.X * 8f) & 0xfff) | xFace;
            verts[i++] = ((int) Math.Round(tl.Y * 8f) & 0xfff) | yFace;
            verts[i++] = tt | (0x0 + ol);
            verts[i++] = ((int) Math.Round(br.X * 8f) & 0xfff) | xFace;
            verts[i++] = ((int) Math.Round(tl.Y * 8f) & 0xfff) | yFace;
            verts[i++] = tt | (nx + ol);
            verts[i++] = ((int) Math.Round(br.X * 8f) & 0xfff) | xFace;
            verts[i++] = ((int) Math.Round(br.Y * 8f) & 0xfff) | yFace;
            verts[i++] = tt | ((nx | ny) + ol);
            verts[i++] = ((int) Math.Round(tl.X * 8f) & 0xfff) | xFace;
            verts[i++] = ((int) Math.Round(br.Y * 8f) & 0xfff) | yFace;
            verts[i++] = tt | (ny + ol);
        }
    }
}
