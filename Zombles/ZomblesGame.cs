using System;
using System.IO;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using ResourceLib;

using Zombles.Graphics;
using Zombles.UI;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles
{
    public class ZomblesGame : GameWindow
    {
        private Stopwatch myTimer;

        public Scene CurrentScene { get; private set; }
        public SpriteShader SpriteShader { get; private set; }

        public double Time
        {
            get { return myTimer.Elapsed.TotalSeconds; }
        }

        public ZomblesGame()
            : base( 800, 600, new GraphicsMode( new ColorFormat( 8, 8, 8, 8 ), 8, 0 ), "Zombles" )
        {
            VSync = VSyncMode.Off;
            // Context.SwapInterval = 1;

            WindowBorder = WindowBorder.Fixed;

            CurrentScene = null;
        }

        protected override void OnLoad( EventArgs e )
        {
            Res.RegisterManager<RScriptManager>();
            Res.RegisterManager<RTexture2DManager>();

            String dataPath = "Data" + Path.DirectorySeparatorChar;
            String loadorderpath = dataPath + "loadorder.txt";

            if ( !File.Exists( loadorderpath ) )
                return;

            foreach ( String line in File.ReadAllLines( loadorderpath ) )
                if ( line.Length > 0 && File.Exists( dataPath + line ) )
                    Res.MountArchive( Res.LoadArchive( dataPath + line ) );

            TextureManager.Initialize();
            Plugin.Initialize();

            SpriteShader = new SpriteShader( Width, Height );

            Mouse.Move += OnMouseMove;
            Mouse.ButtonUp += OnMouseButtonUp;
            Mouse.ButtonDown += OnMouseButtonDown;
            Mouse.WheelChanged += OnMouseWheelChanged;

            myTimer = new Stopwatch();
            myTimer.Start();

            SetScene( new GameScene( this ) );
        }

        public void SetScene( Scene newScene )
        {
            if ( CurrentScene != null )
                CurrentScene.OnExit();
            CurrentScene = newScene;
            if ( CurrentScene != null )
            {
                CurrentScene.OnEnter( CurrentScene.FirstTime );
                CurrentScene.FirstTime = false;
            }
        }

        protected override void OnUpdateFrame( FrameEventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnUpdateFrame( e );

            Plugin.Think( e.Time );
        }

        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            if ( CurrentScene != null )
                CurrentScene.OnRenderFrame( e );

            SwapBuffers();
        }

        private void OnMouseMove( object sender, MouseMoveEventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnMouseMove( e );
        }

        private void OnMouseWheelChanged( object sender, MouseWheelEventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnMouseWheelChanged( e );
        }

        private void OnMouseButtonUp( object sender, MouseButtonEventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnMouseButtonUp( e );
        }

        private void OnMouseButtonDown( object sender, MouseButtonEventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnMouseButtonDown( e );
        }

        protected override void OnKeyPress( KeyPressEventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnKeyPress( e );
        }

        protected override void OnMouseLeave( EventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnMouseLeave( e );
        }

        protected override void OnMouseEnter( EventArgs e )
        {
            if ( CurrentScene != null )
                CurrentScene.OnMouseEnter( e );
        }

        public override void Dispose()
        {
            if ( CurrentScene != null )
                CurrentScene.Dispose();

            base.Dispose();
        }
    }
}
