using System;
using System.Linq;
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
using OpenTKTK.Shaders;
using Zombles.Entities;

namespace Zombles
{
    public class MainWindow : GameWindow
    {
        public const double ThinkFrequency = 60.0;
        public const double ThinkPeriod = 1.0 / ThinkFrequency;

        private static Stopwatch _timer;
        private static Stopwatch _thinkTimer;

        private static double _spareTime;

        public static Scene CurrentScene { get; private set; }
        public static SpriteShader SpriteShader { get; private set; }

        public static double Time
        {
            get { return _timer.Elapsed.TotalSeconds; }
        }

        public MainWindow()
            : base(800, 600, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 16, 0), "Zombles")
        {
            // VSync = VSyncMode.Off;
            Context.SwapInterval = 1;

            // WindowBorder = WindowBorder.Fixed;

            CurrentScene = null;
        }

        protected override void OnLoad(EventArgs e)
        {
            String dataPath = "Data" + Path.DirectorySeparatorChar;
            String loadorderpath = dataPath + "loadorder.txt";

            if (!File.Exists(loadorderpath))
                return;

            Archive.RegisterAll(Assembly.GetExecutingAssembly());

            var resourceDirs = new[] {
                dataPath,
                Path.Combine(dataPath, "..", "..", "..", "Content")
            };

            var archives = File.ReadAllLines(loadorderpath).Where(x => x.Length > 0);

            foreach (var resDir in resourceDirs) {
                foreach (String line in archives.Select(x => Path.Combine(resDir, x + ".dat")))
                    if (File.Exists(line)) Archive.FromFile(line).Mount();

                foreach (String line in archives.Select(x => Path.Combine(resDir, x)))
                    if (line.Length > 0 && Directory.Exists(line))
                        Archive.FromDirectory(line).Mount();
            }

            Mouse.Move += OnMouseMove;
            Mouse.ButtonUp += OnMouseButtonUp;
            Mouse.ButtonDown += OnMouseButtonDown;
            Mouse.WheelChanged += OnMouseWheelChanged;

            _timer = new Stopwatch();
            _timer.Start();

            _thinkTimer = new Stopwatch();
            _thinkTimer.Start();

            _spareTime = 0.0;

            TextureManager.Initialize();
            ScriptManager.Initialize();
            Plugin.Initialize();

            SpriteShader = new SpriteShader(Width, Height);
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
            if (_thinkTimer.Elapsed.TotalSeconds + _spareTime < ThinkPeriod)
                return;

            _spareTime += _thinkTimer.Elapsed.TotalSeconds - ThinkPeriod;

            _thinkTimer.Restart();

            if (CurrentScene != null)
                CurrentScene.OnUpdateFrame(new FrameEventArgs(ThinkPeriod));

            RouteNavigation.Think(ThinkPeriod);
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
