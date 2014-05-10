using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Zombles.Scripts.Entities.Desires
{
    public class PlayerMovementCommand : Desire
    {
        private double _originTime;

        public Vector2 Destination { get; private set; }

        public PlayerMovementCommand(Vector2 dest)
        {
            _originTime = MainWindow.Time;

            Destination = dest;
        }

        public override Intention GetIntention(Beliefs beliefs)
        {
            return new Intentions.PlayerMovementCommand(this, beliefs);
        }

        public override float Utility
        {
            get { return 2048f + (float) ((64f * _originTime) / MainWindow.Time); }
        }

        public override bool ConflictsWith(Desire other)
        {
            return other is Migration || other is Barricading || other is PlayerMovementCommand || other is Mobbing;
        }

        public override Desire ResolveConflict(Desire other)
        {
            return this;
        }
    }
}
