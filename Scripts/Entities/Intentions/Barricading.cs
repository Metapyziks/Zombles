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
        private Tile _destTile;
        private bool _noResources;
        private RouteNavigator _nav;

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
            };

            for (int x = 1; x < block.Width - 1; ++x) {
                for (int y = 1; y < block.Height - 1; ++y) {
                    var tile = block[block.X + x, block.Y + y];

                    if (tile.IsInterior) break;

                    foreach (var dir in dirs) {
                        if (tile.IsWallSolid(dir)) continue;

                        var n = dir.GetNormal();

                        int xn = x + (int) n.X, yn = y + (int) n.Y;
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
            return !ShouldAbandon();
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
                _destTile = null;

                if (_destResource != null && (_destResource.Entity.Position2D != _destResource.LastPos || !_destResource.Entity.IsValid)) {
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
                    } else {
                        if (_nav != null) {
                            _nav.Dispose();
                        }

                        _nav = new RouteNavigator(Entity, _destResource.LastPos +
                            World.Difference(_destResource.LastPos, Entity.Position2D).Normalized() * 1.4f);
                    }
                }

                var diff = World.Difference(Entity.Position2D, _destResource.LastPos);

                if (_nav.HasEnded) {
                    yield return new MovementAction(diff.Normalized() * 2f);
                } else if (_nav.HasDirection) {
                    yield return new MovementAction(_nav.GetDirection() * 2f);
                }

                if (diff.LengthSquared < 2f) {
                    if (_destResource.Type == EntityType.PlankSource) {
                        yield return new AttackAction(_destResource.Entity);
                    } else if (_destResource.Entity.GetComponent<Item>().CanPickup(Entity)) {
                        yield return new PickupItemAction(_destResource.Entity, 2f);
                    }
                }
            } else {
                _destResource = null;

                if (_destTile != null && !_pendingTiles.Contains(_destTile)) {
                    _destTile = null;
                }

                if (_destTile == null) {
                    _destTile = _pendingTiles
                        .OrderBy(x => World.Difference(Entity.Position2D, x.Position).LengthSquared)
                        .FirstOrDefault();

                    if (_nav != null) {
                        _nav.Dispose();
                    }

                    _nav = new RouteNavigator(Entity, _destTile.Position);
                }

                if (_destTile != null) {
                    var diff = World.Difference(Entity.Position2D, _destTile.Position);

                    if (_nav.HasDirection) {
                        yield return new MovementAction(_nav.GetDirection() * 2f);
                    }

                    if (diff.LengthSquared < 0.25f) {
                        yield return new DropItemAction(2f);
                    }
                }
            }
        }
    }
}
