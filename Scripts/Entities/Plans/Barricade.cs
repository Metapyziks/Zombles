using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Plans
{
    class Barricade
    {
        private Entity _leader;
        private Tile[] _destTiles;

        private Tile[] _pendingTiles;

        public bool HasLeader
        {
            get
            {
                if (_leader != null) return false;

                if (!_leader.IsValid || !_leader.GetComponent<Health>().IsAlive) {
                    _leader = null;
                    return false;
                }

                return true;
            }
        }

        public Barricade(Entity leader, IEnumerable<Tile> tiles)
        {
            _leader = leader;

            _destTiles = tiles.ToArray();

            _pendingTiles = _destTiles.Where(x => x.StaticEntities.Count() == 0).ToArray();
        }

        public void Think()
        {
            _pendingTiles = _destTiles.Where(x => x.StaticEntities.Count() == 0).ToArray();
        }
    }
}
