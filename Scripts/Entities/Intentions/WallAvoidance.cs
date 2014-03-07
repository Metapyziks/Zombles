using System;
using System.Collections.Generic;
using OpenTK;
using Zombles.Geometry;
using Zombles.Scripts.Entities;

namespace Zombles.Scripts.Entities.Intentions
{
    class WallAvoidance : Intention
    {
        public WallAvoidance(Desires.WallAvoidance desire, Beliefs beliefs)
            : base(desire, beliefs) { }

        public override bool ShouldAbandon()
        {
            return false;
        }

        public override bool ShouldKeep()
        {
            return false;    
        }

        public override IEnumerable<Action> GetActions()
        {
            var tile = World.GetTile(Entity.Position2D);

            foreach (var face in new[] { Face.North, Face.East, Face.South, Face.West }) {
                if (tile.IsWallSolid(face)) {
                    var pos = new Vector2(tile.X + 0.5f, tile.Y + 0.5f) + face.GetNormal() * 0.5f;
                    var dif = Vector2.Dot(face.GetNormal(), World.Difference(pos, Entity.Position2D));
                    yield return new MovementAction(face.GetNormal() / (dif * 4f));
                }
            }
        }
    }
}
