using Zombles.Entities;

namespace Zombles.Scripts.Entities
{
    public class DeliberativeAI : HumanControl
    {
        private Beliefs _beliefs;

        public DeliberativeAI(Entity ent)
            : base(ent)
        {
            _beliefs = new Beliefs(ent);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
        }

        public override void OnThink(double dt)
        {
            base.OnThink(dt);

            _beliefs.Update();
        }
    }
}
