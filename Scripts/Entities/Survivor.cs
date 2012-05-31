using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zombles.Entities;
using Zombles.Graphics;

namespace Zombles.Scripts.Entities
{
    public class Survivor : Human
    {
        private static EntityAnim stWalkAnim;
        private static EntityAnim stStandAnim;

        public override EntityAnim WalkAnim
        {
            get
            {
                if ( stWalkAnim == null )
                    stWalkAnim = EntityAnim.GetAnim( "human walk" );

                return stWalkAnim;
            }
        }

        public override EntityAnim StandAnim
        {
            get
            {

                if ( stStandAnim == null )
                    stStandAnim = EntityAnim.GetAnim( "human stand" );

                return stStandAnim;
            }
        }

        public Survivor( Entity ent )
            : base( ent )
        {
            MoveSpeed = 1.5f + Tools.Random.NextSingle();
        }
    }
}
