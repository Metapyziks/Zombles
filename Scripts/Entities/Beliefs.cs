using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    internal sealed class Beliefs
    {
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
            private int _survivors;
            private int _zombies;
            private int _resources;

            private double _utility;
            private bool _utilityChanged;

            public Block Block { get; private set; }

            public Beliefs Beliefs { get; private set; }
            
            public double LastSeen { get; private set; }

            public int Survivors { get { return _survivors + Beliefs._entityKB.Count(x => x.Value.Type == EntityType.Survivor && x.Value.LastBlock == Block); } }

            public int Zombies { get { return _zombies + Beliefs._entityKB.Count(x => x.Value.Type == EntityType.Zombie && x.Value.LastBlock == Block); } }

            public int Resources
            {
                get
                {
                    return _resources + Beliefs._entityKB
                        .Where(x => x.Value.Type == EntityType.PlankPile && x.Value.LastBlock == Block)
                        .Sum(x => x.Key.GetComponent<WoodPile>().Count) + Beliefs._entityKB
                        .Where(x => x.Value.Type == EntityType.PlankSource && x.Value.LastBlock == Block)
                        .Select(x => x.Key.GetComponent<WoodenBreakable>())
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

            foreach (var ent in Agent.Block) {
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

            var old = _entityKB.Values.Where(x => MainWindow.Time - x.LastSeen > 10.0).ToArray();

            foreach (var beliefs in old) {
                _entityKB.Remove(beliefs.Entity);
            }
        }
    }
}
