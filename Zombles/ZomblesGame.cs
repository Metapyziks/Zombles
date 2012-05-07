using System;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using ResourceLib;

using Zombles.Graphics;
using Zombles.Geometry;
using Zombles.Geometry.Generation;

namespace Zombles
{
    public class ZomblesGame : GameWindow
    {
        private GeometryShader myGeoShader;
        private City myTestCity;

        private bool myHideTop;

        private float myCamMoveSpeed;

        private bool myIgnoreMouse;
        private bool myCaptureMouse;

        public ZomblesGame()
            : base( 800, 600, new GraphicsMode( new ColorFormat( 8, 8, 8, 0 ), 8, 0 ), "Zombles" )
        {
            myHideTop = false;

            myCamMoveSpeed = 64.0f;

            myIgnoreMouse = false;
            myCaptureMouse = true;
        }

        protected override void OnLoad( EventArgs e )
        {
            Res.RegisterManager<RTexture2DManager>();

            String dataPath = "Data" + Path.DirectorySeparatorChar;
            String loadorderpath = dataPath + "loadorder.txt";

            if ( !File.Exists( loadorderpath ) )
                return;

            foreach ( String line in File.ReadAllLines( loadorderpath ) )
                if ( line.Length > 0 && File.Exists( dataPath + line ) )
                    Res.MountArchive( Res.LoadArchive( dataPath + line ) );

            TileManager.Initialize();

            CityGenerator gen = new CityGenerator();
            gen.AddBlockGenerator( new WarehouseBlockGen(), 1.0 );
            gen.AddBlockGenerator( new EmptyBlockGen(), 0.0 );
            myTestCity = gen.Generate( 256, 256 );

            myGeoShader = new GeometryShader( Width, Height );

            myGeoShader.CameraPosition = new Vector2( 128.0f, 128.0f );
            myGeoShader.CameraRotation = new Vector2( (float) Math.PI * 30.0f / 180.0f, 0.0f );
            myGeoShader.CameraScale = 1.0f;

            Mouse.Move += OnMouseMove;
            Mouse.WheelChanged += OnMouseWheelChanged;

            System.Windows.Forms.Cursor.Hide();
        }

        protected override void OnUpdateFrame( FrameEventArgs e )
        {
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

        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            myGeoShader.StartBatch();
            myTestCity.Render( myGeoShader, myHideTop );
            myGeoShader.EndBatch();

            SwapBuffers();
        }

        private void OnMouseMove( object sender, MouseMoveEventArgs e )
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
                // myUIRoot.SendMouseMoveEvent( new Vector2( Mouse.X, Mouse.Y ), e );
            }
        }

        private void OnMouseWheelChanged( object sender, MouseWheelEventArgs e )
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

        protected override void OnKeyPress( KeyPressEventArgs e )
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
        }

        protected override void OnMouseLeave( EventArgs e )
        {
            if ( myCaptureMouse )
            {
                myIgnoreMouse = true;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point( Bounds.Left + Width / 2, Bounds.Top + Height / 2 );
            }
        }
    }
}
