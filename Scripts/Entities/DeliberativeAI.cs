using System.Collections.Generic;
using System.Linq;

using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public class DeliberativeAI : HumanControl
    {
        public const double DeliberationPeriod = 0.5;

        private Beliefs _beliefs;
        private Intention[] _intentions;
        private double _nextDeliberate;

        public DeliberativeAI(Entity ent)
            : base(ent)
        {
            _beliefs = new Beliefs(ent.GetComponent<Human>());
            _intentions = new Intention[0];
            _nextDeliberate = MainWindow.Time + Tools.Random.NextDouble() * DeliberationPeriod;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
        }

        public override void OnThink(double dt)
        {
            base.OnThink(dt);

            _beliefs.Update();

            if (MainWindow.Time >= _nextDeliberate || _intentions.Any(x => x.ShouldAbandon())) {
                var desires = Desire.DiscoverAll(_beliefs);
                _intentions = desires.Select(x => x.GetIntention(_beliefs)).ToArray();
                _nextDeliberate = MainWindow.Time + DeliberationPeriod;
            }

            foreach (var intention in _intentions) {
                intention.Act();
            }
        }
    }
}
