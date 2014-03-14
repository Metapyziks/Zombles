using System.Collections.Generic;
using System.Linq;

using OpenTK;

using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    public enum EntityType
    {
        Other = 0,
        Survivor = 1,
        Zombie = 2,
        PlankPile = 3,
        PlankSource = 4
    }

    public sealed class EntityBeliefs
    {
        public Entity Entity { get; private set; }

        public EntityType Type { get; private set; }

        public Beliefs Beliefs { get; private set; }

        public Block LastBlock { get; private set; }

        public double LastSeen { get; private set; }

        public Vector2 LastPos { get; private set; }

        public EntityBeliefs(Beliefs beliefs, Entity ent)
        {
            Beliefs = beliefs;
            Entity = ent;

            Type = ent.HasComponent<Survivor>() ? EntityType.Survivor
                : ent.HasComponent<Zombie>() ? EntityType.Zombie
                : ent.HasComponent<WoodPile>() ? EntityType.PlankPile
                : ent.HasComponent<WoodenBreakable>() ? EntityType.PlankSource
                : EntityType.Other;

            Update();
        }

        public EntityBeliefs Clone(Beliefs beliefs)
        {
            return new EntityBeliefs(beliefs, Entity) {
                LastPos = LastPos,
                LastBlock = LastBlock,
                LastSeen = LastSeen
            };
        }

        public void Update()
        {
            LastSeen = MainWindow.Time;
            LastPos = Entity.Position2D;
            LastBlock = Entity.World.GetBlock(LastPos);
        }
    }

    public sealed class BlockBeliefs
    {
        private double _utility;
        private bool _utilityChanged;

        private HashSet<EntityBeliefs> _remembered;

        public Block Block { get; private set; }

        public IEnumerable<EntityBeliefs> Entities { get { return _remembered; } }

        public Beliefs Beliefs { get; private set; }

        public double LastSeen { get; private set; }

        public int Survivors
        {
            get
            {
                return _remembered.Concat(Beliefs.Entities)
                    .Count(x => x.Type == EntityType.Survivor && x.LastBlock == Block);
            }
        }

        public int Zombies
        {
            get
            {
                return _remembered.Concat(Beliefs.Entities)
                    .Count(x => x.Type == EntityType.Zombie && x.LastBlock == Block);
            }
        }

        public int Resources
        {
            get
            {
                var concat = _remembered.Concat(Beliefs.Entities);

                return concat
                    .Where(x => x.Type == EntityType.PlankPile && x.LastBlock == Block)
                    .Where(x => Beliefs.Entity.World.GetTile(x.LastPos).IsInterior)
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

                    _utility = Survivors / 2.0 + Resources / 6.0 - zombies * zombies * 4.0;
                }

                return _utility;
            }
        }

        public BlockBeliefs(Beliefs beliefs, Block block)
        {
            Beliefs = beliefs;
            Block = block;

            _utilityChanged = true;
            _remembered = new HashSet<EntityBeliefs>();

            Update();
        }

        public void CopyFrom(BlockBeliefs other)
        {
            LastSeen = other.LastSeen;

            _remembered.Clear();
            _utilityChanged = true;

            foreach (var ent in other._remembered) {
                _remembered.Add(ent.Clone(Beliefs));
            }
        }

        public void Update()
        {
            LastSeen = MainWindow.Time;

            var trace = new TraceLine(Beliefs.Entity.World) {
                Origin = Beliefs.Entity.Position2D,
                HitGeometry = true,
                HitEntities = false
            };

            var toRemove = new List<EntityBeliefs>();

            foreach (var beliefs in _remembered) {
                if (beliefs.Entity.World.Difference(beliefs.LastPos, Beliefs.Entity.Position2D).LengthSquared > Beliefs.VisibleRange2)
                    continue;

                trace.Target = beliefs.LastPos;

                if (!beliefs.Entity.IsValid || !trace.GetResult().HitWorld) {
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

    public sealed class Beliefs
    {
        public const float VisibleRange = 8f;
        public const float VisibleRange2 = VisibleRange * VisibleRange;

        private Dictionary<Entity, EntityBeliefs> _entityKB;
        private Dictionary<Block, BlockBeliefs> _blockKB;

        public Entity Entity { get; private set; }

        public Human Human { get; private set; }

        public IEnumerable<BlockBeliefs> Blocks { get { return _blockKB.Values; } }

        public IEnumerable<EntityBeliefs> Entities { get { return _entityKB.Values; } }

        public Beliefs(Human human)
        {
            Human = human;
            Entity = human.Entity;

            _entityKB = new Dictionary<Entity, EntityBeliefs>();
            _blockKB = new Dictionary<Block, BlockBeliefs>();

            foreach (var block in Entity.World) {
                _blockKB.Add(block, new BlockBeliefs(this, block));
            }
        }

        private void ReceivePercept(Entity ent)
        {
            if (_entityKB.ContainsKey(ent)) {
                _entityKB[ent].Update();
            } else {
                _entityKB.Add(ent, new EntityBeliefs(this, ent));
            }
        }

        public BlockBeliefs GetBlock(Block block)
        {
            return _blockKB.First(x => x.Key == block).Value;
        }

        public void Update()
        {
            var trace = new TraceLine(Entity.World) {
                Origin = Entity.Position2D,
                HitGeometry = true,
                HitEntities = true
            };

            var nearBlocks = _blockKB.Keys.Where(x =>
                Entity.World.Difference(x.GetNearestPosition(Entity.Position2D),
                    Entity.Position2D).LengthSquared <= VisibleRange2);

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
                    trace.HitEntityPredicate = x => x == ent;

                    var res = trace.GetResult();
                    if (res.HitWorld) continue;

                    ReceivePercept(ent);
                }
            }

            var old = _entityKB.Values.Where(x => MainWindow.Time - x.LastSeen > 10.0 || !x.Entity.IsValid).ToArray();

            foreach (var beliefs in old) {
                _entityKB.Remove(beliefs.Entity);
                _blockKB[beliefs.LastBlock].Remember(beliefs);
            }
        }
    }
}
