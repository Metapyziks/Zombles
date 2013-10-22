using OpenTK;

using Zombles.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.Entities
{
    public class SurvivorAI : HumanControl
    {
        private const double TargetSearchInterval = 0.25;

        private float _fleeRadius;
        private float _runRadius;

        private Vector2 _fleePos;
        private double _lastSearch;

        public SurvivorAI( Entity ent )
            : base( ent )
        {
            _fleeRadius = 4.0f + Tools.Random.NextSingle() * 6.0f;
            _runRadius  = 2.0f + Tools.Random.NextSingle() * 4.0f;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            _fleePos = new Vector2();
            _lastSearch = MainWindow.Time - Tools.Random.NextDouble() * TargetSearchInterval;
        }

        public override void OnThink( double dt )
        {
            if ( !Human.Health.IsAlive )
                return;

            if ( MainWindow.Time - _lastSearch > TargetSearchInterval )
            {
                _lastSearch = MainWindow.Time + Tools.Random.NextSingle() * TargetSearchInterval;

                _fleePos = new Vector2();

                Trace trace = new Trace( City );
                trace.Origin = Position2D;
                trace.HitGeometry = true;
                trace.HitEntities = false;

                NearbyEntityEnumerator it = SearchNearbyEnts( _fleeRadius );
                while ( it.MoveNext() )
                {
                    Entity cur = it.Current;
                    if ( cur.HasComponent<Zombie>() )
                    {
                        Vector2 diff = City.Difference( Position2D, cur.Position2D );
                        float dist2 = diff.LengthSquared;

                        if ( dist2 > 0 )
                        {
                            trace.Target = cur.Position2D;

                            if ( !trace.GetResult().Hit )
                            {
                                _fleePos -= diff / dist2;

                                if ( dist2 < _runRadius * _runRadius )
                                    ( Human as Survivor ).StartRunning();
                            }
                        }
                    }
                }

                if ( _fleePos.LengthSquared == 0.0f )
                {
                    _fleePos.X = Tools.Random.NextSingle() * 2.0f - 1.0f;
                    _fleePos.Y = Tools.Random.NextSingle() * 2.0f - 1.0f;
                }

                Human.StartMoving( _fleePos );
            }
        }
    }
}
