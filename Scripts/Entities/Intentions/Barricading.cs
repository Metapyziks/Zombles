using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Intentions
{
    public class Barricading : Intention
    {
        private BlockBeliefs _blockBeliefs;
        private List<Tile> _destTiles;
        private Tile[] _pendingTiles;
        private EntityBeliefs _destResource;
        private bool _noResources;

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

            _pendingTiles = _destTiles.Where(x => x.StaticEntities.Count() == 0).ToArray();
        }

        public override bool ShouldAbandon()
        {
            return _pendingTiles.Length == 0 || _blockBeliefs.Block.Enclosed || _noResources;
        }

        public override bool ShouldKeep()
        {
            return true;
        }

        public override IEnumerable<Action> GetActions()
        {
            _pendingTiles = _destTiles.Where(x => x.StaticEntities.Count() == 0).ToArray();

            if (ShouldAbandon()) {
                if (Human.IsHoldingItem) {
                    yield return new DropItemAction(1f);
                }

                yield break;
            }

            if (!Human.IsHoldingItem) {
                if (_destResource != null && _destResource.Entity.Position2D != _destResource.LastPos) {
                    _destResource = null;
                }

                if (_destResource == null) {
                    _destResource = _blockBeliefs.Entities
                        .Union(Beliefs.Entities)
                        .Where(x => x.Type == EntityType.PlankSource || x.Type == EntityType.PlankPile)
                        .Where(x => World.GetTile(x.LastPos).IsInterior)
                        .OrderBy(x => World.Difference(x.LastPos, Entity.Position2D).LengthSquared)
                        .FirstOrDefault();

                    if (_destResource == null) {
                        _noResources = true;
                        yield break;
                    }
                }

                yield return new MovementAction(World.Difference(Entity.Position2D, _destResource.LastPos));
            }
        }
    }
}
