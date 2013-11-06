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
    public class PlayerControl : HumanControl
    {
        public PlayerControl(Entity ent)
            : base(ent)
        {

        }

        public override void OnThink(double dt)
        {
            GameScene scene = MainWindow.CurrentScene as GameScene;
            Survivor surv = Human as Survivor;

            if (scene.Mouse[MouseButton.Left]) {
                Vector2 pos = scene.Camera.ScreenToWorld(scene.MousePos, 0.5f);
                surv.Attack(World.Difference(Position2D, pos));
            }

            if (scene.Keyboard[Key.ShiftLeft]) {
                if (surv.CanRun)
                    surv.StartRunning();
            } else if (surv.IsRunning)
                surv.StopRunning();

            Vector2 movement = new Vector2();
            float angleY = scene.Camera.Yaw;

            if (scene.Keyboard[Key.D]) {
                movement.X += (float) Math.Cos(angleY);
                movement.Y += (float) Math.Sin(angleY);
            }
            if (scene.Keyboard[Key.A]) {
                movement.X -= (float) Math.Cos(angleY);
                movement.Y -= (float) Math.Sin(angleY);
            }
            if (scene.Keyboard[Key.S]) {
                movement.Y += (float) Math.Cos(angleY);
                movement.X -= (float) Math.Sin(angleY);
            }
            if (scene.Keyboard[Key.W]) {
                movement.Y -= (float) Math.Cos(angleY);
                movement.X += (float) Math.Sin(angleY);
            }

            if (movement.LengthSquared > 0.0f)
                Human.StartMoving(movement);
            else if (Human.Movement.IsMoving)
                Human.StopMoving();

            scene.Camera.Position2D = Position2D;
        }
    }
}
