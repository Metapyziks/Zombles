using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Intentions
{
    public class Barricading : Intention
    {
        private BlockBeliefs _blockBeliefs;
        private List<Tile> _destTiles;
        private List<Tile> _pendingTiles;

        public Barricading(Desires.Barricading desire, Beliefs beliefs, BlockBeliefs blockBeliefs)
            : base(desire, beliefs)
        {
            _blockBeliefs = blockBeliefs;

            _destTiles = new List<Tile>();

            var block = _blockBeliefs.Block;

            var dirs = new[] {
                Face.North,
                Face.East,
                Face.South,
                Face.West
            }.Select(x => x.GetNormal());

            for (int x = 1; x < block.Width - 1; ++x) {
                for (int y = 1; y < block.Height - 1; ++y) {
                    var tile = block[block.X + x, block.Y + y];

                    if (tile.IsInterior) break;

                    foreach (var dir in dirs) {
                        int xn = x + (int) dir.X, yn = y + (int) dir.Y;
                        var neighbour = block[block.X + xn, block.Y + yn];

                        if (neighbour.IsInterior) {
                            _destTiles.Add(tile);
                            break;
                        }
                    }
                }
            }

            _pendingTiles = _destTiles.Where(x => x.StaticEntities.Count() == 0).ToList();
        }

        public override bool ShouldAbandon()
        {
            return _pendingTiles.Count == 0;
        }

        public override bool ShouldKeep()
        {
            return true;
        }

        public override IEnumerable<Action> GetActions()
        {
            throw new NotImplementedException();
        }
    }
}
