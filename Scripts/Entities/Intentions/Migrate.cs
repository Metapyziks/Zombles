﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;
using Zombles.Scripts.Entities.Desires;

namespace Zombles.Scripts.Entities.Intentions
{
    class Migrate : Intention
    {
        private Block _destBlock;
        private Vector2 _destPos;

        public Migrate(Migration desire, Beliefs beliefs)
            : base(desire, beliefs)
        {
            _destBlock = desire.Destination;
            _destPos = _destBlock.Intersections
                .OrderBy(x => Entity.World.Difference(Entity.Position2D, x.Position).LengthSquared)
                .First()
                .Position;

            _destPos += (_destBlock.Center - _destPos).Normalized();
        }

        public override bool ShouldAbandon()
        {
            return _destBlock == Entity.Block;
        }

        public override bool ShouldKeep()
        {
            return !ShouldAbandon();
        }

        public override void Act()
        {
            var nav = Entity.GetComponent<RouteNavigation>();
            if (!nav.HasRoute || nav.CurrentTarget != _destPos) {
                nav.NavigateTo(_destPos);
            }
        }
    }
}