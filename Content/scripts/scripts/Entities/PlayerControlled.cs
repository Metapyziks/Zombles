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
    public class PlayerControlled : HumanControl
    {
        public PlayerControlled( Entity ent )
            : base( ent )
        {

        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            ( ZomblesGame.CurrentScene as GameScene ).ControlledEnt = Entity;
        }

        public override void OnThink( double dt )
        {
            GameScene scene = ZomblesGame.CurrentScene as GameScene;
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            Survivor surv = Human as Survivor;

            if ( mouse.LeftButton == ButtonState.Pressed )
            {
                Vector2 pos = scene.Camera.ScreenToWorld( scene.MousePos, 0.5f );
                surv.Attack( City.Difference( Position2D, pos ) );
            }

            if ( keyboard[ Key.ShiftLeft ] )
            {
                if ( surv.CanRun )
                    surv.StartRunning();
            }
            else if ( surv.IsRunning )
                surv.StopRunning();

            Vector2 movement = new Vector2();
            float angleY = scene.Camera.Yaw;

            if ( keyboard[ Key.D ] )
            {
                movement.X += (float) Math.Cos( angleY );
                movement.Y += (float) Math.Sin( angleY );
            }
            if ( keyboard[ Key.A ] )
            {
                movement.X -= (float) Math.Cos( angleY );
                movement.Y -= (float) Math.Sin( angleY );
            }
            if ( keyboard[ Key.S ] )
            {
                movement.Y += (float) Math.Cos( angleY );
                movement.X -= (float) Math.Sin( angleY );
            }
            if ( keyboard[ Key.W ] )
            {
                movement.Y -= (float) Math.Cos( angleY );
                movement.X += (float) Math.Sin( angleY );
            }

            if ( movement.LengthSquared > 0.0f )
                Human.StartMoving( movement );
            else if ( Human.Movement.IsMoving )
                Human.StopMoving();

            scene.Camera.Position = Position2D;
        }

        public override void OnRemove()
        {
            ( ZomblesGame.CurrentScene as GameScene ).ControlledEnt = null;
        }
    }
}
