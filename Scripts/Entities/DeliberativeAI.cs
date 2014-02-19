using System.Collections.Generic;
using System.Linq;

using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public class DeliberativeAI : HumanControl
    {
        public const double BeliefsUpdatePeriod = 0.25;
        public const double DeliberationPeriod = 0.5;

        private Beliefs _beliefs;
        private Intention[] _intentions;
        private double _nextBeliefsUpdate;
        private double _nextDeliberate;

        public DeliberativeAI(Entity ent)
            : base(ent)
        {
            _beliefs = new Beliefs(ent.GetComponent<Human>());
            _intentions = new Intention[0];
            _nextBeliefsUpdate = MainWindow.Time + Tools.Random.NextDouble() * BeliefsUpdatePeriod;
            _nextDeliberate = MainWindow.Time + Tools.Random.NextDouble() * DeliberationPeriod;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
        }

        public override void OnThink(double dt)
        {
            base.OnThink(dt);

            bool deliberate = MainWindow.Time >= _nextDeliberate;
            if (MainWindow.Time >= _nextBeliefsUpdate) {
                _beliefs.Update();

                deliberate = deliberate || _intentions.Any(x => x.ShouldAbandon());
                _nextBeliefsUpdate = MainWindow.Time + BeliefsUpdatePeriod;
            }

            if (deliberate) {
                var desires = Desire.DiscoverAll(_beliefs)
                    .Union(_intentions.Select(x => x.Desire));

                desires = Desire.ResolveConflicts(desires);

                var kept = _intentions.Where(x => desires.Contains(x.Desire)).ToArray();
                desires = desires.Where(x => !kept.Any(y => y.Desire == x));

                _intentions = kept.Union(desires.Select(x => x.GetIntention(_beliefs))).ToArray();
                _nextDeliberate = MainWindow.Time + DeliberationPeriod;

                if (_intentions.Length == 0) {
                    Human.StopMoving();
                }
            }

            foreach (var intention in _intentions) {
                intention.Act();
            }
        }
    }
}
