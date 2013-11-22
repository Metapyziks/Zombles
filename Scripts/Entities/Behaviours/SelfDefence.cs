using OpenTK;
using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities.Behaviours
{
    public class SelfDefence : SubsumptionStack.Layer
    {
        public double MinAttackCheckPeriod { get; set; }
        public double MaxAttackCheckPeriod { get; set; }

        public float AttackRadius { get; set; }

        protected double _nextDefendCheck;
        protected Entity _attackTarget;

        protected override void OnSpawn()
        {
            _nextDefendCheck = MainWindow.Time;
            _attackTarget = null;

            MinAttackCheckPeriod = 0.25;
            MaxAttackCheckPeriod = 0.5;

            AttackRadius = 0.75f + Tools.Random.NextSingle() * 1.25f;
        }

        protected override bool OnThink(double dt)
        {
            if (MainWindow.Time >= _nextDefendCheck) {
                _nextDefendCheck = MainWindow.Time + Tools.Random.NextDouble(
                    MinAttackCheckPeriod, MaxAttackCheckPeriod);

                _attackTarget = null;

                float closestDist2 = float.MaxValue;

                var it = SearchNearbyEnts(AttackRadius);
                foreach (var ent in SearchNearbyVisibleEnts(AttackRadius, (ent, diff) =>
                    ent.HasComponent<Zombie>() &&
                    ent.GetComponent<Health>().IsAlive &&
                    diff.LengthSquared > 0 &&
                    diff.LengthSquared < closestDist2)) {

                    _attackTarget = ent;
                    closestDist2 = World.Difference(Entity.Position2D, ent.Position2D).LengthSquared;
                }
            }

            if (_attackTarget != null && _attackTarget.GetComponent<Health>().IsAlive) {
                var diff = World.Difference(Position2D, _attackTarget.Position2D);

                if (Human.CanAttack) {
                    if (diff.LengthSquared < 0.75f) {
                        Human.Attack(diff);
                        Human.StopMoving();
                    } else {
                        Human.StartMoving(diff);
                    }
                }

                return true;
            }

            return false;
        }
    }
}
