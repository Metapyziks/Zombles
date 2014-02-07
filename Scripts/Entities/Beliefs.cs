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
        private class EntityBeliefs
        {
            public Entity Entity { get; private set; }

            public Block LastBlock { get; private set; }

            public double LastSeen { get; private set; }

            public Vector2 LastPos { get; private set; }

            public EntityBeliefs(Entity ent)
            {
                Entity = ent;

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
            public double LastSeen { get; private set; }

            public Block Block { get; private set; }

            public int Survivors { get; private set; }

            public int Zombies { get; private set; }

            public int Resources { get; private set; }

            public double Utility { get; private set; }

            public BlockBeliefs(Block block)
            {
                Block = block;

                Update();
            }

            public void Update()
            {
                LastSeen = MainWindow.Time;

                Survivors = Block.Count(x => x.HasComponent<Survivor>() && x.GetComponent<Health>().IsAlive);
                Zombies = Block.Count(x => x.HasComponent<Zombie>() && x.GetComponent<Health>().IsAlive);
                Resources = Block.Where(x => x.HasComponent<WoodPile>() && Block.World.GetTile(x.Position2D).IsInterior)
                    .Sum(x => x.GetComponent<WoodPile>().Count) + Block.Where(x => x.HasComponent<WoodenBreakable>())
                    .Select(x => x.GetComponent<WoodenBreakable>()).Sum(x => x.MinPlanks + x.MaxPlanks) / 2;

                Utility = Survivors / 2 + Resources / 6 - Zombies * Zombies;
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
                if (hp != null && !hp.IsAlive) continue;

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
