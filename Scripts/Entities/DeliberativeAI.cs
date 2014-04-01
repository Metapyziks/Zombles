using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public class DeliberativeAI : HumanControl
    {
        public const double BeliefsUpdatePeriod = 0.25;
        public const double DeliberationPeriod = 0.5;
        public const double SharePeriod = 1.0;

        private Beliefs _beliefs;
        private Intention[] _intentions;
        private double _nextBeliefsUpdate;
        private double _nextDeliberate;
        private double _nextShare;

        private List<MethodInfo> _desireDiscoveryMethods;

        public DeliberativeAI(Entity ent)
            : base(ent)
        {
            _beliefs = new Beliefs(ent.GetComponent<Human>());
            _intentions = new Intention[0];
            _nextBeliefsUpdate = MainWindow.Time + Tools.Random.NextDouble() * BeliefsUpdatePeriod;
            _nextDeliberate = MainWindow.Time + Tools.Random.NextDouble() * DeliberationPeriod;
            _nextShare = MainWindow.Time + Tools.Random.NextDouble() * SharePeriod;

            _desireDiscoveryMethods = new List<MethodInfo>();
        }

        public DeliberativeAI AddDesire<T>()
            where T : Desire
        {
            var type = typeof(T);
            var discover = type.GetMethod("Discover", BindingFlags.Public | BindingFlags.Static);

            if (!typeof(IEnumerable<Desire>).IsAssignableFrom(discover.ReturnType)) throw new ArgumentException();
            if (discover.GetParameters().Length != 1) throw new ArgumentException();
            if (discover.GetParameters().First().ParameterType != typeof(Beliefs)) throw new ArgumentException();

            _desireDiscoveryMethods.Add(discover);

            return this;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
        }

        public override void OnThink(double dt)
        {
            base.OnThink(dt);

            if (Human.IsSelected) System.Diagnostics.Debugger.Break();

            bool deliberate = MainWindow.Time >= _nextDeliberate;
            if (MainWindow.Time >= _nextBeliefsUpdate) {
                _beliefs.Update();

                deliberate = deliberate || _intentions.Any(x => x.ShouldAbandon());
                _nextBeliefsUpdate = MainWindow.Time + BeliefsUpdatePeriod * (0.5 + Tools.Random.NextDouble());

                if (MainWindow.Time > _nextShare) {
                    var nearest = _beliefs.Entities
                        .Where(x => x.Entity.HasComponent<DeliberativeAI>()
                            && World.Difference(Position2D, x.LastPos).LengthSquared < 8f)
                        .Select(x => x.Entity)
                        .OrderBy(x => World.Difference(x.Position2D, Entity.Position2D).LengthSquared)
                        .FirstOrDefault();

                    if (nearest != null) {
                        var beliefs = nearest.GetComponent<DeliberativeAI>()._beliefs;

                        var blockPair = _beliefs.Blocks
                            .GroupJoin(beliefs.Blocks, x => x.Block, x => x.Block, (x, y) => Tuple.Create(x, y.First()))
                            .OrderBy(x => x.Item2.LastSeen - x.Item1.LastSeen)
                            .First();

                        if (blockPair.Item1.LastSeen > blockPair.Item2.LastSeen) {
                            blockPair.Item2.CopyFrom(blockPair.Item1);
                        }
                    }

                    _nextShare = MainWindow.Time + SharePeriod * (0.5 + Tools.Random.NextDouble());
                }
            }

            if (deliberate) {
                var desires = _desireDiscoveryMethods
                    .SelectMany(x => (IEnumerable<Desire>) x.Invoke(null, new[] { _beliefs }))
                    .ToList()
                    .Union(_intentions
                        .Where(x => x.ShouldKeep())
                        .Select(x => x.Desire));

                desires = Desire.ResolveConflicts(desires);

                var kept = _intentions.Where(x => desires.Contains(x.Desire)).ToArray();
                var abandoned = _intentions.Where(x => !desires.Contains(x.Desire)).ToArray();

                desires = desires.Where(x => !kept.Any(y => y.Desire == x));

                foreach (var intention in abandoned) {
                    intention.Abandon();
                }

                _intentions = kept.Union(desires.Select(x => x.GetIntention(_beliefs))).ToArray();
                _nextDeliberate = MainWindow.Time + DeliberationPeriod * (0.5 + Tools.Random.NextDouble());

                if (_intentions.Length == 0) {
                    Human.StopMoving();
                }
            }

            var actions = Action.ResolveConflicts(_intentions.SelectMany(x => x.GetActions()));

            if (actions.OfType<MovementAction>().Count() == 0) {
                Human.StopMoving();
            }

            foreach (var action in actions) {
                action.Perform(Human);
            }
        }
    }
}
