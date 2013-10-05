using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;

using Zombles.UI;
using Zombles.Graphics;
using Zombles.Entities;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

using Zombles.Scripts.UI;

namespace Zombles.Scripts
{
    public class GameScene : Scene
    {
        public const int WorldSize = 128;

        private UILabel _fpsText;
        private UILabel _posText;
        private UIInfectionDisplay _infDisplay;

        private long _totalFrameTime;
        private int _framesCompleted;

        private Stopwatch _frameTimer;

        private bool _hideTop;

        private float _camMoveSpeed;
        private int _camDir;
        private DateTime _camRotTime;
        private int _oldCamDir;
        private bool _mapView;
        private bool _drawPathNetwork;

        private DebugTraceShader _traceShader;

        private float TargetCameraPitch
        {
            get { return _mapView ? MathHelper.Pi * 90.0f / 180.0f : MathHelper.Pi * 30.0f / 180.0f; }
        }
        private float PreviousCameraPitch
        {
            get { return _mapView ? MathHelper.Pi * 30.0f / 180.0f : MathHelper.Pi * 90.0f / 180.0f; }
        }

        private float TargetCameraYaw
        {
            get { return ( ( _camDir % 16 ) * 180.0f / 8.0f - 180.0f ) * MathHelper.Pi / 180.0f; }
        }
        private float PreviousCameraYaw
        {
            get { return ( ( _oldCamDir % 16 ) * 180.0f / 8.0f - 180.0f ) * MathHelper.Pi / 180.0f; }
        }

        public Camera Camera { get; private set; }
        public GeometryShader GeoShader { get; private set; }
        public FlatEntityShader FlatEntShader { get; private set; }
        public CityGenerator Generator { get; private set; }
        public City City { get; private set; }

        public List<Entity> SelectedEntities { get; private set; }

        public GameScene( ZomblesGame gameWindow )
            : base( gameWindow )
        {
            _hideTop = false;

            _camMoveSpeed = 16.0f;
            _camDir = 2;
            _camRotTime = DateTime.MinValue;
            _mapView = false;
            _drawPathNetwork = false;

            _totalFrameTime = 0;
            _framesCompleted = 0;
            _frameTimer = new Stopwatch();
        }

        public override void OnEnter( bool firstTime )
        {
            base.OnEnter( firstTime );

            if ( firstTime )
            {
                _fpsText = new UILabel( Font.Large );
                _fpsText.Colour = Color4.White;
                AddChild( _fpsText );

                _posText = new UILabel( Font.Large );
                _posText.Colour = Color4.White;
                AddChild( _posText );

                _infDisplay = new UIInfectionDisplay();
                AddChild( _infDisplay );

                PositionUI();

                Generator = new CityGenerator();
                City = Generator.Generate( WorldSize, WorldSize );

                SelectedEntities = new List<Entity>();

                Camera = new Camera( Width, Height, 4.0f );
                Camera.SetWrapSize( WorldSize, WorldSize );
                Camera.Position = new Vector2( WorldSize, WorldSize ) / 2.0f;
                Camera.Pitch = TargetCameraPitch;
                Camera.Yaw = TargetCameraYaw;

                Plugin.CityGenerated();

                GeoShader = new GeometryShader();
                GeoShader.Camera = Camera;

                FlatEntShader = new FlatEntityShader();
                FlatEntShader.Camera = Camera;

                _traceShader = new DebugTraceShader();
                _traceShader.Camera = Camera;

                Camera.UpdatePerspectiveMatrix();

                _frameTimer.Start();
            }
        }

        public override void OnResize()
        {
            Camera.SetScreenSize( Width, Height );

            PositionUI();
        }

        private void PositionUI()
        {
            _fpsText.Left = 4.0f;
            _fpsText.Top = 4.0f;

            _posText.Left = 4.0f;
            _posText.Top = 8.0f + Font.Large.CharHeight;

            _infDisplay.Width = Width - 8.0f;
            _infDisplay.Height = 8.0f;
            _infDisplay.Left = 4.0f;
            _infDisplay.Top = Height - 4.0f - _infDisplay.Height;
        }

        public override void OnUpdateFrame( FrameEventArgs e )
        {
            if ( _totalFrameTime >= Stopwatch.Frequency )
            {
                double period = _totalFrameTime / ( Stopwatch.Frequency / 1000d ) / _framesCompleted;
                double freq = 1000 / period;

                _totalFrameTime = 0;
                _framesCompleted = 0;

                _fpsText.Text = string.Format( "FT: {0:F}ms FPS: {1:F} MEM: {2:F}MB",
                    period, freq, Process.GetCurrentProcess().PrivateMemorySize64 / ( 1024d * 1024d ) );
            }

            _posText.Text = string.Format( "X: {0:F} Y: {1:F}", Camera.Position.X, Camera.Position.Y );

            City.Think( e.Time );            

            _infDisplay.UpdateBars();

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
                    (float) ( e.Time * _camMoveSpeed * ( Keyboard[ Key.ShiftLeft ] ? 4.0f : 1.0f ) );
            }

            if ( ( DateTime.Now - _camRotTime ).TotalSeconds >= 0.25 )
            {
                if ( Camera.Yaw != TargetCameraYaw )
                    Camera.Yaw = TargetCameraYaw;
                if ( Camera.Pitch != TargetCameraPitch )
                    Camera.Pitch = TargetCameraPitch;

                if ( Keyboard[ Key.M ] )
                {
                    _mapView = !_mapView;
                    if ( _mapView )
                    {
                        _oldCamDir = _camDir;
                        _camDir = 0;
                    }
                    else
                    {
                        _camDir = _oldCamDir;
                        _oldCamDir = 0;
                    }
                    _camRotTime = DateTime.Now;
                }
                else if ( !_mapView && ( Keyboard[ Key.Q ] || Keyboard[ Key.E ] ) )
                {
                    _oldCamDir = _camDir;
                    _camRotTime = DateTime.Now;

                    if ( Keyboard[ Key.Q ] )
                        _camDir = ( _camDir + 1 ) % 16;
                    if ( Keyboard[ Key.E ] )
                        _camDir = ( _camDir + 15 ) % 16;
                }
            }
            else
            {
                float rdiff = Tools.AngleDif( TargetCameraYaw, PreviousCameraYaw );
                float pdiff = Tools.AngleDif( TargetCameraPitch, PreviousCameraPitch );
                float time = (float) ( ( DateTime.Now - _camRotTime ).TotalSeconds / 0.25 );

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
                City.RenderGeometry( GeoShader, _hideTop );
                GeoShader.EndBatch();
                FlatEntShader.StartBatch();
                City.RenderEntities( FlatEntShader );
                FlatEntShader.EndBatch();
                _traceShader.Begin();
                _traceShader.Colour = Color4.Red;
                foreach ( Entity ent in SelectedEntities )
                {
                    if ( ent.HasComponent<PathNavigation>() && ent.HasComponent<Health>() )
                    {
                        PathNavigation nav = ent.GetComponent<PathNavigation>();
                        Health health = ent.GetComponent<Health>();
                        if( nav.CurrentPath != null && health.IsAlive )
                            _traceShader.Render( nav.CurrentPath );
                    }
                }
                _traceShader.Colour = new Color4( 255, 255, 255, 127 );
                _traceShader.End();
                if ( _drawPathNetwork )
                {
                    _traceShader.StartBatch();
                    City.RenderPaths( _traceShader );
                    _traceShader.EndBatch();
                }
            }

            base.OnRenderFrame( e );

            _totalFrameTime += _frameTimer.ElapsedTicks;
            ++_framesCompleted;
            _frameTimer.Restart();
        }

        public override void OnMouseWheelChanged( MouseWheelEventArgs e )
        {
            if ( e.DeltaPrecise >= 0.0f )
                Camera.Scale *= 1.0f + ( e.DeltaPrecise / 4.0f );
            else
                Camera.Scale /= 1.0f - ( e.DeltaPrecise / 4.0f );

            _camMoveSpeed = 64.0f / Camera.Scale;
        }

        public override void OnKeyPress( KeyPressEventArgs e )
        {
            switch( char.ToLower( e.KeyChar ) )
            {
                case 'x':
                    _hideTop = !_hideTop;
                    break;
                case 'p':
                    _drawPathNetwork = !_drawPathNetwork;
                    break;
                case 'f':
                    if ( GameWindow.WindowState == WindowState.Fullscreen )
                        GameWindow.WindowState = WindowState.Normal;
                    else
                        GameWindow.WindowState = WindowState.Fullscreen;
                    break;
                case 'g':
                    City = Generator.Generate( WorldSize, WorldSize );
                    Plugin.CityGenerated();
                    break;
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
