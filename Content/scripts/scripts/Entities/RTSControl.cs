using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;

using Zombles.Entities;
using Zombles.Graphics;

namespace Zombles.Scripts.Entities
{
    public class RTSControl : HumanControl
    {
        public PathNavigation PathNavigation { get; private set; }

        public bool Selected
        {
            get { return ( (GameScene) ZomblesGame.CurrentScene ).SelectedEntities.Contains( Entity ); }
        }

        public RTSControl( Entity ent )
            : base( ent )
        {

        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            PathNavigation = null;

            if( Entity.HasComponent<PathNavigation>() )
                PathNavigation = Entity.GetComponent<PathNavigation>();

            ZomblesGame.CurrentScene.MouseButtonDown += MouseButtonDown;
        }

        private void MouseButtonDown( object sender, MouseButtonEventArgs e )
        {
            GameScene scene = ZomblesGame.CurrentScene as GameScene;
            if ( e.Button == MouseButton.Right && PathNavigation != null && Selected )
                PathNavigation.NavigateTo( scene.Camera.ScreenToWorld( new Vector2( e.X, e.Y ) ) );
        }

        public override void OnThink( double dt )
        {
            if ( !Human.Health.IsAlive )
            {
                Entity.RemoveComponent<PathNavigation>();
                Entity.UpdateComponents();
            }
            else
            {
                if ( PathNavigation != null && PathNavigation.CurrentPath != null )
                    Human.StartMoving( City.Difference( Position2D, PathNavigation.NextWaypoint ) );
                else if ( Human.Movement.IsMoving )
                    Human.StopMoving();
            }
        }

        public override void OnRemove()
        {
            ZomblesGame.CurrentScene.MouseButtonDown -= MouseButtonDown;
        }
    }
}
