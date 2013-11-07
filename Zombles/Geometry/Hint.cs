using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Zombles.Geometry
{
    public abstract class Hint
    {
        public Vector2 Position { get; private set; }

        protected Hint(Vector2 position)
        {
            Position = position;
        }

        public abstract Vector2 Influence(Vector2 target);
    }

    public class CornerHint : Hint
    {
        private static Vector2 FindCorner(Tile tile)
        {
            var pos = new Vector2(tile.X, tile.Y);

            if (tile.IsWallSolid(Face.East)) {
                pos.X += 1;
            }

            if (tile.IsWallSolid(Face.South)) {
                pos.Y += 1;
            }

            return pos;
        }

        private Vector2 _attractor;
        private float _radius;

        public CornerHint(Tile tile, float radius = 3f)
            : base(FindCorner(tile))
        {
            var diff = new Vector2(tile.X + 0.5f, tile.Y + 0.5f) - Position;

            diff.X = Math.Sign(diff.X);
            diff.Y = Math.Sign(diff.Y);

            _radius = radius;
            _attractor = Position + diff * _radius;
        }

        public override Vector2 Influence(Vector2 target)
        {
            if (target.X < Math.Min(Position.X, _attractor.X)) return Vector2.Zero;
            if (target.Y < Math.Min(Position.Y, _attractor.Y)) return Vector2.Zero;
            if (target.X >= Math.Max(Position.X, _attractor.X)) return Vector2.Zero;
            if (target.Y >= Math.Max(Position.Y, _attractor.Y)) return Vector2.Zero;

            var diff = target - _attractor;

            if (diff.LengthSquared <= _radius * _radius) return Vector2.Zero;

            var dist = diff.Length;
            return (diff * (dist - _radius)).Normalized();
        }
    }
}
