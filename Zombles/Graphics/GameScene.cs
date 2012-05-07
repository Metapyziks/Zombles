using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;

using ResourceLib;

using Zombles.UI;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles.Graphics
{
    public class GameScene : Scene
    {
        private const int WorldSize = 256;

        private UILabel myFPSText;

        private long myTotalFrameTime;
        private int myFramesCompleted;

        private Stopwatch myFrameTimer;

        private GeometryShader myGeoShader;
        private CityGenerator myGenerator;
        private City myTestCity;

        private bool myHideTop;

        private float myCamMoveSpeed;

        private bool myIgnoreMouse;
        private bool myCaptureMouse;

        public GameScene( ZomblesGame gameWindow )
            : base( gameWindow )
        {
            myHideTop = false;

            myCamMoveSpeed = 64.0f;

            myIgnoreMouse = false;
            myCaptureMouse = true;

            myTotalFrameTime = 0;
            myFramesCompleted = 0;
            myFrameTimer = new Stopwatch();
        }

        public override void OnEnter( bool firstTime )
        {
            base.OnEnter( firstTime );

            if ( firstTime )
            {
                myFPSText = new UILabel( Font.Large, new Vector2( 4.0f, 4.0f ) );
                myFPSText.Colour = Color4.White;
                AddChild( myFPSText );

                myGenerator = new CityGenerator();
                myGenerator.AddBlockGenerator( new WarehouseBlockGen(), 1.0 );
                myGenerator.AddBlockGenerator( new EmptyBlockGen(), 0.0 );
                myTestCity = myGenerator.Generate( WorldSize, WorldSize );

                myGeoShader = new GeometryShader( Width, Height );

                myGeoShader.CameraPosition = new Vector2( WorldSize, WorldSize ) / 2.0f;
                myGeoShader.CameraRotation = new Vector2( (float) Math.PI * 30.0f / 180.0f, 0.0f );
                myGeoShader.CameraScale = 1.0f;

                System.Windows.Forms.Cursor.Hide();

                myFrameTimer.Start();
            }
        }

        public override void OnUpdateFrame( FrameEventArgs e )
        {
            if ( myTotalFrameTime >= Stopwatch.Frequency )
            {
                double period = myTotalFrameTime / ( Stopwatch.Frequency / 1000d ) / myFramesCompleted;
                double freq = 1000 / period;

                myTotalFrameTime = 0;
                myFramesCompleted = 0;

                myFPSText.Text = string.Format( "FT: {0:F}ms FPS: {1:F} MEM: {2:F}MB", period, freq, Process.GetCurrentProcess().PrivateMemorySize64 / ( 1024d * 1024d ) );
            }

            Vector2 movement = new Vector2( 0.0f, 0.0f );
            float angleY = myGeoShader.CameraRotation.Y;

            if ( Keyboard[ Key.D ] )
            {
                movement.X += (float) Math.Cos( angleY );
                movement.Y += (float) Math.Sin( angleY );
            }
            if ( Keyboard[ Key.A ] )
            {
                movement.X -= (float) Math.Cos( angleY );
                movement.Y -= (float) Math.Sin( angleY );
            }
            if ( Keyboard[ Key.S ] )
            {
                movement.Y += (float) Math.Cos( angleY );
                movement.X -= (float) Math.Sin( angleY );
            }
            if ( Keyboard[ Key.W ] )
            {
                movement.Y -= (float) Math.Cos( angleY );
                movement.X += (float) Math.Sin( angleY );
            }

            if ( movement.Length != 0 )
            {
                movement.Normalize();
                myGeoShader.CameraPosition = myGeoShader.CameraPosition + movement *
                    (float) ( e.Time * myCamMoveSpeed * ( Keyboard[ Key.ShiftLeft ] ? 4.0f : 1.0f ) );
            }
        }

        public override void OnRenderFrame( FrameEventArgs e )
        {
            myGeoShader.StartBatch();
            myTestCity.Render( myGeoShader, myHideTop );
            myGeoShader.EndBatch();

            base.OnRenderFrame( e );

            myTotalFrameTime += myFrameTimer.ElapsedTicks;
            ++myFramesCompleted;
            myFrameTimer.Restart();
        }

        public override void OnMouseMove( MouseMoveEventArgs e )
        {
            if ( myIgnoreMouse )
            {
                myIgnoreMouse = false;
                return;
            }

            if ( myCaptureMouse )
            {
                Vector2 rot = myGeoShader.CameraRotation;

                rot.Y += e.XDelta / 180.0f;

                myGeoShader.CameraRotation = rot;

                myIgnoreMouse = true;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point( Bounds.Left + Width / 2, Bounds.Top + Height / 2 );
            }
            else
            {
                base.OnMouseMove( e );
            }
        }

        public override void OnMouseWheelChanged( MouseWheelEventArgs e )
        {
            if ( myCaptureMouse )
            {
                if ( e.DeltaPrecise >= 0.0f )
                    myGeoShader.CameraScale *= 1.0f + ( e.DeltaPrecise / 4.0f );
                else
                    myGeoShader.CameraScale /= 1.0f - ( e.DeltaPrecise / 4.0f );

                myCamMoveSpeed = 64.0f / myGeoShader.CameraScale;
            }
        }

        public override void OnKeyPress( KeyPressEventArgs e )
        {
            if ( e.KeyChar == 0x1b )
            {
                myCaptureMouse = !myCaptureMouse;
                if ( myCaptureMouse )
                    System.Windows.Forms.Cursor.Hide();
                else
                    System.Windows.Forms.Cursor.Show();
            }
            else if ( e.KeyChar == 'x' )
            {
                myHideTop = !myHideTop;
            }
            else if ( e.KeyChar == 'g' )
            {
                myTestCity = myGenerator.Generate( WorldSize, WorldSize );
            }

            if ( !myCaptureMouse )
            {
                base.OnKeyPress( e );
            }
        }

        public override void OnMouseLeave( EventArgs e )
        {
            if ( myCaptureMouse )
            {
                myIgnoreMouse = true;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point( Bounds.Left + Width / 2, Bounds.Top + Height / 2 );
            }
        }

        public override void Dispose()
        {
            myTestCity.Dispose();

            base.Dispose();
        }
    }
}
