using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;

using Zombles.Graphics;
using Zombles.Entities;

using OpenTKTK.Utils;

namespace Zombles.Geometry
{
    public class District : IEnumerable<Block>
    {
        private List<Entity> _unspawnedEnts;

        public World World { get; private set; }

        public Rectangle Bounds { get; private set; }
        public int LongSide { get; private set; }
        public int ShortSide { get; private set; }

        public int X { get { return Bounds.X; } }
        public int Y { get { return Bounds.Y; } }
        public int Width { get { return Bounds.Width; } }
        public int Height { get { return Bounds.Height; } }

        public bool IsRoot { get; private set; }
        public bool IsBranch { get; private set; }
        public bool IsHorzSplit { get; private set; }
        public bool IsLeaf { get; private set; }

        public int Depth { get; private set; }

        public District Parent { get; private set; }

        public District ChildA { get; private set; }
        public District ChildB { get; private set; }

        public Block Block { get; private set; }

        public District(World world, int x, int y, int width, int height)
            : this((District) null, x, y, width, height)
        {
            World = world;
        }

        private District(District parent, int x, int y, int width, int height)
        {
            if (parent != null)
                World = parent.World;

            Bounds = new Rectangle(x, y, width, height);
            LongSide = Math.Max(Width, Height);
            ShortSide = Math.Min(Width, Height);

            IsRoot = parent == null;
            IsBranch = false;
            IsLeaf = false;

            Depth = 1;

            Parent = parent;

            _unspawnedEnts = new List<Entity>();
        }

        public void PlaceEntity(Entity ent)
        {
            _unspawnedEnts.Add(ent);
        }

        public Block GetBlock(float x, float y)
        {
            if (IsLeaf)
                return Block;

            if (IsBranch) {
                if (IsHorzSplit) {
                    if (y >= ChildB.Y)
                        return ChildB.GetBlock(x, y);
                    return ChildA.GetBlock(x, y);
                } else {
                    if (x >= ChildB.X)
                        return ChildB.GetBlock(x, y);
                    return ChildA.GetBlock(x, y);
                }
            }

            return null;
        }

        public void Split(bool horizontal, int offset)
        {
            if (offset < 1)
                throw new ArgumentOutOfRangeException("Cannot split with an offset less than 1.");

            IsBranch = true;
            IsLeaf = false;
            IsHorzSplit = horizontal;
            if (horizontal) {
                if (offset > Height - 1)
                    throw new ArgumentOutOfRangeException("Cannot split with an offset greater than "
                        + (Height - 1) + ".");

                ChildA = new District(this, X, Y, Width, offset);
                ChildB = new District(this, X, Y + offset, Width, Height - offset);
            } else {
                if (offset > Width - 1)
                    throw new ArgumentOutOfRangeException("Cannot split with an offset greater than "
                        + (Width - 1) + ".");

                ChildA = new District(this, X, Y, offset, Height);
                ChildB = new District(this, X + offset, Y, Width - offset, Height);
            }

            SetDepth(Depth + 1);
        }

        private void SetDepth(int newDepth)
        {
            if (Depth < newDepth) {
                Depth = newDepth;
                if (Parent != null)
                    Parent.SetDepth(newDepth + 1);
            }
        }

        public void SetBlock(Block block)
        {
            IsLeaf = true;
            Block = block;

            foreach (Entity ent in _unspawnedEnts)
                ent.Spawn();

            _unspawnedEnts.Clear();
        }

        public int GetGeometryVertexCount()
        {
            if (IsBranch)
                return ChildA.GetGeometryVertexCount() + ChildB.GetGeometryVertexCount();
            else if (IsLeaf)
                return Block.GetGeometryVertexCount();

            return 0;
        }

        public void GetGeometryVertices(float[] verts, ref int i)
        {
            if (IsBranch) {
                ChildA.GetGeometryVertices(verts, ref i);
                ChildB.GetGeometryVertices(verts, ref i);
            } else if (IsLeaf)
                Block.GetGeometryVertices(verts, ref i);
        }

        public void Think(double dt)
        {
            if (IsBranch) {
                ChildA.Think(dt);
                ChildB.Think(dt);
            } else if (IsLeaf)
                Block.Think(dt);
        }

        public void PostThink()
        {
            if (IsBranch) {
                ChildA.PostThink();
                ChildB.PostThink();
            } else if (IsLeaf)
                Block.PostThink();
        }

        public IEnumerable<Block> GetVisibleBlocks(OrthoCamera camera)
        {
            if (IsBranch) {
                if (ChildA.Bounds.IntersectsWith(camera.ViewBounds)) {
                    foreach (var block in ChildA.GetVisibleBlocks(camera)) {
                        yield return block;
                    }
                }
                if (ChildB.Bounds.IntersectsWith(camera.ViewBounds)) {
                    foreach (var block in ChildB.GetVisibleBlocks(camera)) {
                        yield return block;
                    }
                }
            } else if (IsLeaf) {
                yield return Block;
            }
        }

        public IEnumerator<Block> GetEnumerator()
        {
            return new DistrictEnumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
