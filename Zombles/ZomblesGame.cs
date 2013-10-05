using System;
using System.IO;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using ResourceLibrary;

using Zombles.Graphics;
using Zombles.UI;
using Zombles.Geometry;
using Zombles.Geometry.Generation;
using System.Reflection;

namespace Zombles
{
    public class ZomblesGame : GameWindow
    {
        public const double ThinkFrequency = 60.0;
        public const double ThinkPeriod = 1.0 / ThinkFrequency;

        private static Stopwatch myTimer;
        private static Stopwatch myThinkTimer;

        private static double mySpareTime;

        public static Scene CurrentScene { get; private set; }
        public static SpriteShader SpriteShader { get; private set; }

        public static double Time
        {
            get { return myTimer.Elapsed.TotalSeconds; }
        }

        public ZomblesGame()
            : base(800, 600, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 16, 0), "Zombles")
        {
            VSync = VSyncMode.Off;
            // Context.SwapInterval = 1;

            WindowBorder = WindowBorder.Fixed;

            CurrentScene = null;
        }

        protected override void OnLoad(EventArgs e)
        {
            String dataPath = "Data" + Path.DirectorySeparatorChar;
            String loadorderpath = dataPath + "loadorder.txt";

            if (!File.Exists(loadorderpath))
                return;

            Archive.RegisterAll(Assembly.GetExecutingAssembly());

            foreach (String line in File.ReadAllLines(loadorderpath))
                if (line.Length > 0 && File.Exists(dataPath + line))
                    Archive.FromFile(dataPath + line).Mount();

            TextureManager.Initialize();
            ScriptManager.Initialize();
            Plugin.Initialize();

            SpriteShader = new SpriteShader(Width, Height);

            Mouse.Move += OnMouseMove;
            Mouse.ButtonUp += OnMouseButtonUp;
            Mouse.ButtonDown += OnMouseButtonDown;
            Mouse.WheelChanged += OnMouseWheelChanged;

            myTimer = new Stopwatch();
            myTimer.Start();

            myThinkTimer = new Stopwatch();
            myThinkTimer.Start();

            mySpareTime = 0.0;

            SetScene(ScriptManager.CreateInstance<Scene>("Zombles.Scripts.GameScene", this));
        }

        protected override void OnResize(EventArgs e)
        {
            SpriteShader.SetScreenSize(Width, Height);

            CurrentScene.OnResize();

            GL.Viewport(ClientRectangle);
        }

        public static void SetScene(Scene newScene)
        {
            if (CurrentScene != null)
                CurrentScene.OnExit();
            CurrentScene = newScene;
            if (CurrentScene != null) {
                CurrentScene.OnEnter(CurrentScene.FirstTime);
                CurrentScene.FirstTime = false;
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (myThinkTimer.Elapsed.TotalSeconds + mySpareTime < ThinkPeriod)
                return;

            mySpareTime += myThinkTimer.Elapsed.TotalSeconds - ThinkPeriod;

            myThinkTimer.Restart();

            if (CurrentScene != null)
                CurrentScene.OnUpdateFrame(new FrameEventArgs(ThinkPeriod));

            Plugin.Think(ThinkPeriod);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (CurrentScene != null)
                CurrentScene.OnRenderFrame(e);

            SwapBuffers();
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            if (CurrentScene != null)
                CurrentScene.TriggerMouseMove(e);
        }

        private void OnMouseWheelChanged(object sender, MouseWheelEventArgs e)
        {
            if (CurrentScene != null)
                CurrentScene.OnMouseWheelChanged(e);
        }

        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentScene != null)
                CurrentScene.TriggerMouseButtonUp(e);
        }

        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentScene != null)
                CurrentScene.TriggerMouseButtonDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (CurrentScene != null)
                CurrentScene.OnKeyPress(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (CurrentScene != null)
                CurrentScene.OnMouseLeave(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (CurrentScene != null)
                CurrentScene.OnMouseEnter(e);
        }

        public override void Dispose()
        {
            if (CurrentScene != null)
                CurrentScene.Dispose();

            base.Dispose();
        }
    }
}
