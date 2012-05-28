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
using Zombles.Graphics;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles
{
    public class GameScene : Scene
    {
        private const int WorldSize = 256;

        private UILabel myFPSText;

        private long myTotalFrameTime;
        private int myFramesCompleted;

        private Stopwatch myFrameTimer;

        private Camera myCamera;
        private GeometryShader myGeoShader;
        private FlatEntityShader myFlatEntShader;
        private CityGenerator myGenerator;
        private City myTestCity;

        private bool myHideTop;

        private float myCamMoveSpeed;
        private int myCamDir;
        private DateTime myCamRotTime;
        private int myOldCamDir;
        private bool myMapView;

        private float TargetCameraPitch
        {
            get { return myMapView ? MathHelper.Pi * 90.0f / 180.0f : MathHelper.Pi * 30.0f / 180.0f; }
        }
        private float PreviousCameraPitch
        {
            get { return myMapView ? MathHelper.Pi * 30.0f / 180.0f : MathHelper.Pi * 90.0f / 180.0f; }
        }

        private float TargetCameraYaw
        {
            get { return ( ( myCamDir % 16 ) * 180.0f / 8.0f - 180.0f ) * MathHelper.Pi / 180.0f; }
        }
        private float PreviousCameraYaw
        {
            get { return ( ( myOldCamDir % 16 ) * 180.0f / 8.0f - 180.0f ) * MathHelper.Pi / 180.0f; }
        }

        private bool myIgnoreMouse;
        private bool myCaptureMouse;

        public GameScene( ZomblesGame gameWindow )
            : base( gameWindow )
        {
            myHideTop = false;

            myCamMoveSpeed = 64.0f;
            myCamDir = 2;
            myCamRotTime = DateTime.MinValue;
            myMapView = false;

            myIgnoreMouse = false;
            myCaptureMouse = false;

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
                myTestCity = myGenerator.Generate( WorldSize, WorldSize );

                myCamera = new Camera( Width, Height, 4.0f );
                myCamera.SetWrapSize( WorldSize, WorldSize );
                myCamera.Position = new Vector2( WorldSize, WorldSize ) / 2.0f;
                myCamera.Yaw = TargetCameraYaw;

                myGeoShader = new GeometryShader();
                myGeoShader.Camera = myCamera;

                myFlatEntShader = new FlatEntityShader();
                myFlatEntShader.Camera = myCamera;

                myCamera.UpdatePerspectiveMatrix();

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
            float angleY = myCamera.Yaw;

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
                myCamera.Position += movement *
                    (float) ( e.Time * myCamMoveSpeed * ( Keyboard[ Key.ShiftLeft ] ? 4.0f : 1.0f ) );
            }

            if ( ( DateTime.Now - myCamRotTime ).TotalSeconds >= 0.25 )
            {
                if ( myCamera.Yaw != TargetCameraYaw )
                    myCamera.Yaw = TargetCameraYaw;
                if ( myCamera.Pitch != TargetCameraPitch )
                    myCamera.Pitch = TargetCameraPitch;

                if ( Keyboard[ Key.M ] )
                {
                    myMapView = !myMapView;
                    if ( myMapView )
                    {
                        myOldCamDir = myCamDir;
                        myCamDir = 0;
                    }
                    else
                    {
                        myCamDir = myOldCamDir;
                        myOldCamDir = 0;
                    }
                    myCamRotTime = DateTime.Now;
                }
                else if ( !myMapView && ( Keyboard[ Key.Q ] || Keyboard[ Key.E ] ) )
                {
                    myOldCamDir = myCamDir;
                    myCamRotTime = DateTime.Now;

                    if ( Keyboard[ Key.Q ] )
                        myCamDir = ( myCamDir + 1 ) % 16;
                    if ( Keyboard[ Key.E ] )
                        myCamDir = ( myCamDir + 15 ) % 16;
                }
            }
            else
            {
                float rdiff = Tools.AngleDif( TargetCameraYaw, PreviousCameraYaw );
                float pdiff = Tools.AngleDif( TargetCameraPitch, PreviousCameraPitch );
                float time = (float) ( ( DateTime.Now - myCamRotTime ).TotalSeconds / 0.25 );

                myCamera.Yaw = Tools.WrapAngle( PreviousCameraYaw + rdiff * time );

                if ( myCamera.Pitch != TargetCameraPitch )
                    myCamera.Pitch = Tools.WrapAngle( PreviousCameraPitch + pdiff * time );
            }

            myCamera.UpdatePerspectiveMatrix();
            myCamera.UpdateViewMatrix();
        }

        public override void OnRenderFrame( FrameEventArgs e )
        {
            float x0 = 0.0f;
            float x1 = ( myCamera.Position.X < WorldSize / 2 ) ? -WorldSize : WorldSize;
            float y0 = 0.0f;
            float y1 = ( myCamera.Position.Y < WorldSize / 2 ) ? -WorldSize : WorldSize;

            for ( int i = 0; i < 4; ++i )
            {
                myCamera.WorldHorizontalOffset = ( i & 0x1 ) == 0x0 ? x0 : x1;
                myCamera.WorldVerticalOffset = ( i & 0x2 ) == 0x0 ? y0 : y1;
                myCamera.UpdateViewBoundsOffset();
                myGeoShader.StartBatch();
                myTestCity.RenderGeometry( myGeoShader, myHideTop );
                myGeoShader.EndBatch();
                myFlatEntShader.StartBatch();
                myTestCity.RenderEntities( myFlatEntShader );
                myFlatEntShader.EndBatch();
            }

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
            if ( e.DeltaPrecise >= 0.0f )
                myCamera.Scale *= 1.0f + ( e.DeltaPrecise / 4.0f );
            else
                myCamera.Scale /= 1.0f - ( e.DeltaPrecise / 4.0f );

            myCamMoveSpeed = 64.0f / myCamera.Scale;
        }

        public override void OnKeyPress( KeyPressEventArgs e )
        {
            if ( e.KeyChar == 'x' )
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
