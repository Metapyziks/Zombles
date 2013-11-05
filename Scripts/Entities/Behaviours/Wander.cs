using OpenTK;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class Wander : SubsumptionStack.Layer
    {
        public double MinRedirectPeriod { get; set; }
        public double MaxRedirectPeriod { get; set; }

        private double _nextDirTime;
        private Vector2 _wanderDir;

        protected override void OnSpawn()
        {
            _nextDirTime = MainWindow.Time;

            MinRedirectPeriod = 0.5;
            MaxRedirectPeriod = 2.0;
        }

        protected override bool OnThink(double dt)
        {
            if (MainWindow.Time >= _nextDirTime) {
                _nextDirTime = MainWindow.Time + Tools.Random.NextDouble(MinRedirectPeriod, MaxRedirectPeriod);
                _wanderDir = new Vector2(Tools.Random.NextSingle(-1f, 1f), Tools.Random.NextSingle(-1f, 1f));
            }

            Human.StartMoving(_wanderDir);
            return true;
        }
    }
}
