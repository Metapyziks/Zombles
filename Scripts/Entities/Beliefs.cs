using System.Collections.Generic;
using System.Linq;

using OpenTK;

using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    internal sealed class Beliefs
    {
        private const float VisibleRange = 16f;
        private const float VisibleRange2 = VisibleRange * VisibleRange;

        private enum EntityType
        {
            Other = 0,
            Survivor = 1,
            Zombie = 2,
            PlankPile = 3,
            PlankSource = 4
        }

        private class EntityBeliefs
        {
            public Entity Entity { get; private set; }

            public EntityType Type { get; private set; }

            public Beliefs Beliefs { get; private set; }

            public Block LastBlock { get; private set; }

            public double LastSeen { get; private set; }

            public Vector2 LastPos { get; private set; }

            public EntityBeliefs(Entity ent)
            {
                Entity = ent;
                Type = ent.HasComponent<Survivor>() ? EntityType.Survivor
                    : ent.HasComponent<Zombie>() ? EntityType.Zombie
                    : ent.HasComponent<WoodPile>() ? EntityType.PlankPile
                    : ent.HasComponent<WoodenBreakable>() ? EntityType.PlankSource
                    : EntityType.Other;

                Update();
            }

            public void Update()
            {
                LastSeen = MainWindow.Time;
                LastPos = Entity.Position2D;
                LastBlock = Entity.World.GetBlock(LastPos);
            }
        }

        private class BlockBeliefs
        {
            private double _utility;
            private bool _utilityChanged;

            private HashSet<EntityBeliefs> _remembered;

            public Block Block { get; private set; }

            public Beliefs Beliefs { get; private set; }
            
            public double LastSeen { get; private set; }

            public int Survivors
            {
                get
                {
                    return _remembered.Concat(Beliefs._entityKB.Values)
                        .Count(x => x.Type == EntityType.Survivor && x.LastBlock == Block);
                }
            }

            public int Zombies
            {
                get
                {
                    return _remembered.Concat(Beliefs._entityKB.Values)
                        .Count(x => x.Type == EntityType.Zombie && x.LastBlock == Block);
                }
            }

            public int Resources
            {
                get
                {
                    var concat = _remembered.Concat(Beliefs._entityKB.Values);

                    return concat
                        .Where(x => x.Type == EntityType.PlankPile && x.LastBlock == Block)
                        .Sum(x => x.Entity.GetComponent<WoodPile>().Count) + concat
                        .Where(x => x.Type == EntityType.PlankSource && x.LastBlock == Block)
                        .Select(x => x.Entity.GetComponent<WoodenBreakable>())
                        .Sum(x => x.MinPlanks + x.MaxPlanks) / 2;
                }
            }

            public double Utility
            {
                get
                {
                    if (_utilityChanged) {
                        _utilityChanged = false;

                        int zombies = Zombies;

                        _utility = Survivors / 2.0 + Resources / 6.0 - zombies * zombies;
                    }

                    return _utility; 
                }
            }

            public BlockBeliefs(Block block)
            {
                Block = block;
                _utilityChanged = true;

                Update();
            }

            public void Update()
            {
                LastSeen = MainWindow.Time;

                var trace = new TraceLine(Beliefs.Agent.World) {
                    Origin = Beliefs.Agent.Position2D,
                    HitGeometry = true,
                    HitEntities = false
                };

                var toRemove = new List<EntityBeliefs>();

                foreach (var beliefs in _remembered) {
                    if (beliefs.Entity.World.Difference(beliefs.LastPos, Beliefs.Agent.Position2D).LengthSquared > VisibleRange2)
                        continue;

                    trace.Target = beliefs.LastPos;

                    if (!trace.GetResult().HitWorld) {
                        toRemove.Add(beliefs);
                    }
                }

                foreach (var beliefs in toRemove) {
                    _remembered.Remove(beliefs);
                    _utilityChanged = true;
                }
            }

            public void Remember(EntityBeliefs beliefs)
            {
                _remembered.Add(beliefs);
                _utilityChanged = true;
            }
        }

        private Dictionary<Entity, EntityBeliefs> _entityKB;
        private Dictionary<Block, BlockBeliefs> _blockKB;

        public Entity Agent { get; private set; }

        public Beliefs(Entity agent)
        {
            Agent = agent;

            _entityKB = new Dictionary<Entity, EntityBeliefs>();
            _blockKB = new Dictionary<Block, BlockBeliefs>();

            foreach (var block in Agent.World) {
                _blockKB.Add(block, new BlockBeliefs(block));
            }
        }

        private void ReceivePercept(Entity ent)
        {
            if (_entityKB.ContainsKey(ent)) {
                _entityKB[ent].Update();
            } else {
                _entityKB.Add(ent, new EntityBeliefs(ent));
            }
        }

        public void Update()
        {
            var trace = new TraceLine(Agent.World) {
                Origin = Agent.Position2D,
                HitGeometry = true,
                HitEntities = false
            };

            var nearBlocks = _blockKB.Keys.Where(x => x.Intersections.Any(y =>
                Agent.World.Difference(y.Position, Agent.Position2D).LengthSquared <= VisibleRange2));

            foreach (var block in nearBlocks) {
                _blockKB[block].Update();

                foreach (var ent in block) {
                    var hp = ent.GetComponentOrNull<Health>();
                    if (hp != null && !hp.IsAlive) {
                        if (_entityKB.ContainsKey(ent)) {
                            _entityKB.Remove(ent);
                        }
                        continue;
                    }

                    trace.Target = ent.Position2D;

                    var res = trace.GetResult();
                    if (res.HitWorld) continue;

                    ReceivePercept(ent);
                }
            }

            var old = _entityKB.Values.Where(x => MainWindow.Time - x.LastSeen > 10.0).ToArray();

            foreach (var beliefs in old) {
                _entityKB.Remove(beliefs.Entity);
                _blockKB[beliefs.LastBlock].Remember(beliefs);
            }
        }
    }
}
