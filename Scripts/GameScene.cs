﻿using System;
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

using Zombles.Scripts.UI;

namespace Zombles.Scripts
{
    public class GameScene : Scene
    {
        public const int WorldSize = 256;

        private UILabel myFPSText;
        private UIInfectionDisplay myInfDisplay;

        private long myTotalFrameTime;
        private int myFramesCompleted;

        private Stopwatch myFrameTimer;

        public Camera Camera { get; private set; }
        public GeometryShader GeoShader { get; private set; }
        public BloodShader BloodShader { get; private set; }
        public FlatEntityShader FlatEntShader { get; private set; }
        public CityGenerator Generator { get; private set; }
        public City City { get; private set; }

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

        public GameScene( ZomblesGame gameWindow )
            : base( gameWindow )
        {
            myHideTop = false;

            myCamMoveSpeed = 64.0f;
            myCamDir = 2;
            myCamRotTime = DateTime.MinValue;
            myMapView = false;

            myTotalFrameTime = 0;
            myFramesCompleted = 0;
            myFrameTimer = new Stopwatch();
        }

        public override void OnEnter( bool firstTime )
        {
            base.OnEnter( firstTime );

            if ( firstTime )
            {
                myFPSText = new UILabel( Font.Large );
                myFPSText.Colour = Color4.White;
                AddChild( myFPSText );

                myInfDisplay = new UIInfectionDisplay();
                AddChild( myInfDisplay );

                PositionUI();

                Generator = new CityGenerator();
                City = Generator.Generate( WorldSize, WorldSize );
                Plugin.CityGenerated();

                Camera = new Camera( Width, Height, 4.0f );
                Camera.SetWrapSize( WorldSize, WorldSize );
                Camera.Position = new Vector2( WorldSize, WorldSize ) / 2.0f;
                Camera.Yaw = TargetCameraYaw;

                GeoShader = new GeometryShader();
                GeoShader.Camera = Camera;

                BloodShader = new BloodShader();
                BloodShader.WorldSize = new Vector2( City.Width, City.Height );
                BloodShader.Camera = Camera;

                FlatEntShader = new FlatEntityShader();
                FlatEntShader.Camera = Camera;

                Camera.UpdatePerspectiveMatrix();

                myFrameTimer.Start();
            }
        }

        public override void OnResize()
        {
            Camera.SetScreenSize( Width, Height );

            PositionUI();
        }

        private void PositionUI()
        {
            myFPSText.Left = 4.0f;
            myFPSText.Top = 4.0f;

            myInfDisplay.Width = Width - 8.0f;
            myInfDisplay.Height = 8.0f;
            myInfDisplay.Left = 4.0f;
            myInfDisplay.Top = Height - 4.0f - myInfDisplay.Height;
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

            City.Think( e.Time );

            myInfDisplay.UpdateBars();

            Vector2 movement = new Vector2( 0.0f, 0.0f );
            float angleY = Camera.Yaw;

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
                Camera.Position += movement *
                    (float) ( e.Time * myCamMoveSpeed * ( Keyboard[ Key.ShiftLeft ] ? 4.0f : 1.0f ) );
            }

            if ( ( DateTime.Now - myCamRotTime ).TotalSeconds >= 0.25 )
            {
                if ( Camera.Yaw != TargetCameraYaw )
                    Camera.Yaw = TargetCameraYaw;
                if ( Camera.Pitch != TargetCameraPitch )
                    Camera.Pitch = TargetCameraPitch;

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

                Camera.Yaw = Tools.WrapAngle( PreviousCameraYaw + rdiff * time );

                if ( Camera.Pitch != TargetCameraPitch )
                    Camera.Pitch = Tools.WrapAngle( PreviousCameraPitch + pdiff * time );
            }

            Camera.UpdatePerspectiveMatrix();
            Camera.UpdateViewMatrix();
        }

        public override void OnRenderFrame( FrameEventArgs e )
        {
            float x0 = 0.0f;
            float x1 = ( Camera.Position.X < WorldSize / 2 ) ? -WorldSize : WorldSize;
            float y0 = 0.0f;
            float y1 = ( Camera.Position.Y < WorldSize / 2 ) ? -WorldSize : WorldSize;

            for ( int i = 0; i < 4; ++i )
            {
                Camera.WorldHorizontalOffset = ( i & 0x1 ) == 0x0 ? x0 : x1;
                Camera.WorldVerticalOffset = ( i & 0x2 ) == 0x0 ? y0 : y1;
                Camera.UpdateViewBoundsOffset();
                GeoShader.StartBatch();
                City.RenderGeometry( GeoShader, myHideTop );
                GeoShader.EndBatch();
                FlatEntShader.StartBatch();
                City.RenderEntities( FlatEntShader );
                FlatEntShader.EndBatch();
            }
            
            for ( int i = 0; i < 4; ++i )
            {
                Camera.WorldHorizontalOffset = ( i & 0x1 ) == 0x0 ? x0 : x1;
                Camera.WorldVerticalOffset = ( i & 0x2 ) == 0x0 ? y0 : y1;
                Camera.UpdateViewBoundsOffset();
                BloodShader.Begin();
                City.RenderBlood( BloodShader );
                BloodShader.End();
            }

            base.OnRenderFrame( e );

            myTotalFrameTime += myFrameTimer.ElapsedTicks;
            ++myFramesCompleted;
            myFrameTimer.Restart();
        }

        public override void OnMouseWheelChanged( MouseWheelEventArgs e )
        {
            if ( e.DeltaPrecise >= 0.0f )
                Camera.Scale *= 1.0f + ( e.DeltaPrecise / 4.0f );
            else
                Camera.Scale /= 1.0f - ( e.DeltaPrecise / 4.0f );

            myCamMoveSpeed = 64.0f / Camera.Scale;
        }

        public override void OnKeyPress( KeyPressEventArgs e )
        {
            if ( char.ToLower( e.KeyChar ) == 'x' )
            {
                myHideTop = !myHideTop;
            }
            else if ( char.ToLower( e.KeyChar ) == 'f' )
            {
                if ( GameWindow.WindowState == WindowState.Fullscreen )
                    GameWindow.WindowState = WindowState.Normal;
                else
                    GameWindow.WindowState = WindowState.Fullscreen;
            }
            else if ( char.ToLower( e.KeyChar ) == 'g' )
            {
                City = Generator.Generate( WorldSize, WorldSize );
                Plugin.CityGenerated();
            }

            base.OnKeyPress( e );
        }

        public override void Dispose()
        {
            City.Dispose();

            base.Dispose();
        }
    }
}