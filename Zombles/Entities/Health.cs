using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zombles.Entities
{
    public class HealedEventArgs : EventArgs
    {
        public readonly int Healed;

        public HealedEventArgs( int healed )
        {
            Healed = healed;
        }
    }

    public class DamagedEventArgs : EventArgs
    {
        public readonly int Damage;
        public readonly Entity Attacker;

        public bool HasAttacker
        {
            get { return Attacker != null; }
        }

        public DamagedEventArgs( int damage, Entity attacker = null )
        {
            Damage = damage;
            Attacker = attacker;
        }
    }

    public class KilledEventArgs : EventArgs
    {
        public readonly Entity Attacker;

        public bool HasAttacker
        {
            get { return Attacker != null; }
        }

        public KilledEventArgs( Entity attacker = null )
        {
            Attacker = attacker;
        }
    }

    public class Health : Component
    {
        public int MaxHealth { get; private set; }
        public int Value { get; private set; }

        public bool IsAlive { get; private set; }

        public event EventHandler<HealedEventArgs> Healed;
        public event EventHandler<DamagedEventArgs> Damaged;
        public event EventHandler<KilledEventArgs> Killed;

        public Health( Entity ent )
            : base( ent )
        {
            MaxHealth = 1;
            Value = 1;

            IsAlive = true;
        }

        public void Revive()
        {
            Value = MaxHealth;
            IsAlive = true;

            if ( Healed != null )
                Healed( this, new HealedEventArgs( MaxHealth ) );
        }

        public void SetMaximum( int value )
        {
            MaxHealth = value;

            if ( Value > MaxHealth )
                Value = MaxHealth;
        }

        public void Heal( int value )
        {
            if ( value > 0 )
            {
                Value += value;

                if ( Value > MaxHealth )
                    Value = MaxHealth;

                if ( Healed != null )
                    Healed( this, new HealedEventArgs( value ) );
            }
        }

        public void Damage( int value, Entity attacker = null )
        {
            if ( value > 0 )
            {
                Value -= value;

                if ( Damaged != null )
                    Damaged( this, new DamagedEventArgs( value, attacker ) );

                if ( Value <= 0 )
                {
                    Value = 0;
                    IsAlive = false;

                    if ( Killed != null )
                        Killed( this, new KilledEventArgs( attacker ) );
                }
            }
        }
    }
}
