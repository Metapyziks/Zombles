using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Reflection;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using OpenTKTK.Shaders;

using ResourceLibrary;

using Zombles.Graphics;
using Zombles.Entities;

namespace Zombles
{
    public class MainWindow : GameWindow
    {
        private class DebugListener : TraceListener
        {
            private static readonly String LogPath = "debug.log";

            public DebugListener()
            {
                File.Create(LogPath).Close();
            }

            public override void Write(string message)
            {
                File.AppendAllText(LogPath, message);
            }

            public override void WriteLine(string message)
            {
                File.AppendAllText(LogPath, message + Environment.NewLine);
            }
        }

        public const double ThinkFrequency = 60.0;
        public const double ThinkPeriod = 1.0 / ThinkFrequency;

        private static Stopwatch _timer;
        private static Stopwatch _thinkTimer;
        private static double _normalizedTime;

        private static double _spareTime;

        public static Scene CurrentScene { get; private set; }
        public static SpriteShader SpriteShader { get; private set; }

        public static double Time
        {
            get { return _normalizedTime; }
        }

        public MainWindow()
            : base(800, 600, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 16, 0), "Zombles")
        {
            VSync = VSyncMode.Off;

            Trace.Listeners.Add(new DebugListener());

            CurrentScene = null;
        }

        protected override void OnLoad(EventArgs e)
        {
            VSync = VSyncMode.On;

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

            Keyboard.KeyDown += OnKeyDown;

            Mouse.Move += OnMouseMove;
            Mouse.ButtonUp += OnMouseButtonUp;
            Mouse.ButtonDown += OnMouseButtonDown;
            Mouse.WheelChanged += OnMouseWheelChanged;

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
#if !TURBO
            if (_thinkTimer.Elapsed.TotalSeconds + _spareTime < ThinkPeriod)
                return;

            _spareTime += _thinkTimer.Elapsed.TotalSeconds - ThinkPeriod;
#endif
            _thinkTimer.Restart();
#if TURBO
            do {
#endif
                _normalizedTime += ThinkPeriod;
            
                if (CurrentScene != null)
                    CurrentScene.OnUpdateFrame(new FrameEventArgs(ThinkPeriod));

                Plugin.Think(ThinkPeriod);
#if TURBO
            } while (_thinkTimer.Elapsed.TotalSeconds <= 1.0 / 60.0);
#endif
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

        protected void OnKeyDown(object sender, KeyboardKeyEventArgs e)
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
