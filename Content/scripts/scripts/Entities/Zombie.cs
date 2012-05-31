using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Zombles.Entities;
using Zombles.Graphics;

namespace Zombles.Scripts.Entities
{
    public class Zombie : Human
    {
        private static EntityAnim stWalkAnim;
        private static EntityAnim stStandAnim;

        public override EntityAnim WalkAnim
        {
            get
            {
                if ( stWalkAnim == null )
                    stWalkAnim = EntityAnim.GetAnim( "zombie walk" );

                return stWalkAnim;
            }
        }

        public override EntityAnim StandAnim
        {
            get
            {

                if ( stStandAnim == null )
                    stStandAnim = EntityAnim.GetAnim( "zombie stand" );

                return stStandAnim;
            }
        }

        public Zombie( Entity ent )
            : base( ent )
        {
            MoveSpeed = 1.0f + Tools.Random.NextSingle();
        }
    }
}
