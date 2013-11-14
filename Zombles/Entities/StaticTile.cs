using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Geometry;

namespace Zombles.Entities
{
    public class StaticTile : Component
    {
        private Tile _tile;

        public StaticTile(Entity ent)
            : base(ent)
        {
            _tile = null;
        }

        private void UpdateTile(Tile newTile)
        {
            if (newTile == _tile) return;

            if (_tile != null) {
                _tile.RemoveStaticEntity(Entity);
            }

            _tile = newTile;

            if (_tile != null) {
                _tile.AddStaticEntity(Entity);
            }
        }

        public override void OnSpawn()
        {
            UpdateTile(World.GetTile(Position2D));
        }

        public override void OnRemove()
        {
            UpdateTile(null);
        }
    }
}
