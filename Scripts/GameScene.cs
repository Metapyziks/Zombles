using System;
using System.Linq;
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
using Zombles.Scripts.Entities;

using OpenTKTK.Utils;
using OpenTKTK.Textures;
using OpenTKTK.Scene;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Zombles.Scripts
{
    public class GameScene : Scene
    {
        private MenuScene _menu;

        private UILabel _fpsText;
        private UILabel _posText;
        private UIInfectionDisplay _infDisplay;

        private long _totalFrameTime;
        private int _framesCompleted;

        private Stopwatch _frameTimer;

        private bool _hideTop;

        private bool _isSelecting;
        private Vector2 _selectionStart;

        private float _camScale;
        private int _camDir;
        private DateTime _camRotTime;
        private int _oldCamDir;

        private bool _mapView;
        private bool _drawPathNetwork;
        private bool _drawTestTrace;

        private DebugTraceShader _traceShader;

        private List<Tile> _playerBarricades;

        private int _upScale = 1;

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
            get { return ((_camDir % 16) * 180.0f / 8.0f - 180.0f) * MathHelper.Pi / 180.0f; }
        }
        private float PreviousCameraYaw
        {
            get { return ((_oldCamDir % 16) * 180.0f / 8.0f - 180.0f) * MathHelper.Pi / 180.0f; }
        }

        public OrthoCamera Camera { get; private set; }
        public GeometryShader GeoShader { get; private set; }
        public FlatEntityShader FlatEntShader { get; private set; }
        public ModelEntityShader ModelEntShader { get; private set; }
        public CityGenerator Generator { get; private set; }
        public World World { get; private set; }

        public int WorldSize { get; private set; }

        public int HumanCount { get; private set; }

        public int ZombieCount { get; private set; }

        public IEnumerable<Tile> PlayerBarricades { get { return _playerBarricades; } }

        public GameScene(MainWindow gameWindow, MenuScene menu)
            : base(gameWindow)
        {
            _menu = menu;

            _hideTop = false;

            _camScale = 4f;
            _camDir = 2;
            _camRotTime = DateTime.MinValue;
            _mapView = false;

            _totalFrameTime = 0;
            _framesCompleted = 0;
            _frameTimer = new Stopwatch();

            WorldSize = menu.WorldSize;
            HumanCount = menu.HumanCount;
            ZombieCount = menu.ZombieCount;
        }

        public override void OnEnter(bool firstTime)
        {
            base.OnEnter(firstTime);

            if (firstTime) {
                Generator = new CityGenerator();

                World = Generator.Generate(WorldSize, WorldSize);

                _fpsText = new UILabel(PixelFont.Large);
                _fpsText.Colour = Color4.White;
                AddChild(_fpsText);

                _posText = new UILabel(PixelFont.Large);
                _posText.Colour = Color4.White;
                AddChild(_posText);

                _infDisplay = new UIInfectionDisplay(World);
                AddChild(_infDisplay);

                PositionUI();

                Camera = new OrthoCamera(Width, Height, 4.0f / _upScale);
                Camera.SetWrapSize(WorldSize, WorldSize);
                Camera.Position2D = new Vector2(WorldSize, WorldSize) / 2.0f;
                Camera.Pitch = TargetCameraPitch;
                Camera.Yaw = TargetCameraYaw;

                Plugin.CityGenerated();

                GeoShader = new GeometryShader();
                GeoShader.Camera = Camera;

                FlatEntShader = new FlatEntityShader();
                FlatEntShader.Camera = Camera;

                ModelEntShader = new ModelEntityShader();
                ModelEntShader.Camera = Camera;

                _traceShader = new DebugTraceShader();
                _traceShader.Camera = Camera;

                _frameTimer.Start();
            }
        }

        public override void OnResize()
        {
            Camera.SetScreenSize(Width, Height);

            PositionUI();
        }

        private void PositionUI()
        {
            _fpsText.Left = 4.0f;
            _fpsText.Top = 4.0f;

            _posText.Left = 4.0f;
            _posText.Top = 8.0f + PixelFont.Large.CharHeight;

            _infDisplay.Width = Width - 8.0f;
            _infDisplay.Height = 8.0f;
            _infDisplay.Left = 4.0f;
            _infDisplay.Top = Height - 4.0f - _infDisplay.Height;
        }

        public override void OnUpdateFrame(FrameEventArgs e)
        {
            if (_totalFrameTime >= Stopwatch.Frequency) {
                double period = _totalFrameTime / (Stopwatch.Frequency / 1000d) / _framesCompleted;
                double freq = 1000 / period;

                _totalFrameTime = 0;
                _framesCompleted = 0;

                _fpsText.Text = string.Format("FT: {0:F}ms FPS: {1:F} MEM: {2:F}MB",
                    period, freq, Process.GetCurrentProcess().PrivateMemorySize64 / (1024d * 1024d));
            }

            _posText.Text = string.Format("X: {0:F} Y: {1:F}", Camera.X, Camera.Z);

            World.Think(e.Time);

            _infDisplay.UpdateBars();

            Vector2 movement = new Vector2(0.0f, 0.0f);
            float angleY = Camera.Yaw;

            if (Keyboard[Key.D]) {
                movement.X += (float) Math.Cos(angleY);
                movement.Y += (float) Math.Sin(angleY);
            }
            if (Keyboard[Key.A]) {
                movement.X -= (float) Math.Cos(angleY);
                movement.Y -= (float) Math.Sin(angleY);
            }
            if (Keyboard[Key.S]) {
                movement.Y += (float) Math.Cos(angleY);
                movement.X -= (float) Math.Sin(angleY);
            }
            if (Keyboard[Key.W]) {
                movement.Y -= (float) Math.Cos(angleY);
                movement.X += (float) Math.Sin(angleY);
            }

            if (movement.Length != 0) {
                movement.Normalize();
                Camera.Position2D += movement *
                    (float) (e.Time * (64f / (Camera.Scale * _upScale)) * (Keyboard[Key.ShiftLeft] ? 4.0f : 1.0f));
            }

            if (Math.Abs(Camera.Scale - _camScale) < 1f / 128f) {
                if (Camera.Scale != _camScale) {
                    Camera.Scale = _camScale;
                }
            } else {
                Camera.Scale += (_camScale - Camera.Scale) * 0.25f;
            }

            if ((DateTime.Now - _camRotTime).TotalSeconds >= 0.25) {
                if (Camera.Yaw != TargetCameraYaw)
                    Camera.Yaw = TargetCameraYaw;
                if (Camera.Pitch != TargetCameraPitch)
                    Camera.Pitch = TargetCameraPitch;

                if (Keyboard[Key.M]) {
                    _mapView = !_mapView;
                    if (_mapView) {
                        _oldCamDir = _camDir;
                        _camDir = 0;
                    } else {
                        _camDir = _oldCamDir;
                        _oldCamDir = 0;
                    }
                    _camRotTime = DateTime.Now;
                } else if (!_mapView && (Keyboard[Key.Q] || Keyboard[Key.E])) {
                    _oldCamDir = _camDir;
                    _camRotTime = DateTime.Now;

                    if (Keyboard[Key.Q])
                        _camDir = (_camDir + 1) % 16;
                    if (Keyboard[Key.E])
                        _camDir = (_camDir + 15) % 16;
                }
            } else {
                float rdiff = Tools.AngleDif(TargetCameraYaw, PreviousCameraYaw);
                float pdiff = Tools.AngleDif(TargetCameraPitch, PreviousCameraPitch);
                float time = (float) ((DateTime.Now - _camRotTime).TotalSeconds / 0.25);

                Camera.Yaw = Tools.WrapAngle(PreviousCameraYaw + rdiff * time);

                if (Camera.Pitch != TargetCameraPitch)
                    Camera.Pitch = Tools.WrapAngle(PreviousCameraPitch + pdiff * time);
            }            
        }

        public override void OnRenderFrame(FrameEventArgs e)
        {
            float x0 = 0.0f;
            float x1 = (Camera.X < WorldSize / 2) ? -WorldSize : WorldSize;
            float y0 = 0.0f;
            float y1 = (Camera.Z < WorldSize / 2) ? -WorldSize : WorldSize;

            float hullSize = .5f;

            TraceResult trace = null;

            if (_drawTestTrace) {
                trace = new Zombles.Geometry.TraceLine(World) {
                    Origin = Camera.Position2D,
                    HullSize = new Vector2(hullSize, hullSize),
                    HitGeometry = true,
                    HitEntities = false,
                    Length = 32f,
                    Normal = World.Difference(Camera.Position2D,
                        Camera.ScreenToWorld(new Vector2(Mouse.X, Mouse.Y) / _upScale, 0.5f))
                }.GetResult();
            }

            var hs = hullSize / 2f;

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            for (int i = 0; i < 4; ++i) {
                Camera.WorldOffsetX = (i & 0x1) == 0x0 ? x0 : x1;
                Camera.WorldOffsetY = (i & 0x2) == 0x0 ? y0 : y1;

                World.RenderGeometry(GeoShader, _hideTop);

                FlatEntShader.Begin();
                World.RenderEntities(FlatEntShader);
                FlatEntShader.End();

                ModelEntShader.Begin();
                World.RenderEntities(ModelEntShader);
                ModelEntShader.End();

                _traceShader.Begin(true);

                if (_drawPathNetwork) {
                    World.RenderIntersectionNetwork(_traceShader);
                }

                if (_drawTestTrace) {
                    _traceShader.Render(trace);
                    _traceShader.Render(trace.End.X - hs, trace.End.Y - hs, trace.End.X + hs, trace.End.Y - hs);
                    _traceShader.Render(trace.End.X + hs, trace.End.Y - hs, trace.End.X + hs, trace.End.Y + hs);
                    _traceShader.Render(trace.End.X + hs, trace.End.Y + hs, trace.End.X - hs, trace.End.Y + hs);
                    _traceShader.Render(trace.End.X - hs, trace.End.Y + hs, trace.End.X - hs, trace.End.Y - hs);
                }

                if (_isSelecting) {
                    var pos = Camera.ScreenToWorld(new Vector2(Mouse.X, Mouse.Y) / _upScale, .5f);
                    var diff = World.Difference(pos, _selectionStart);
                    _traceShader.Render(pos.X, pos.Y, pos.X + diff.X, pos.Y);
                    _traceShader.Render(pos.X + diff.X, pos.Y, pos.X + diff.X, pos.Y + diff.Y);
                    _traceShader.Render(pos.X + diff.X, pos.Y + diff.Y, pos.X, pos.Y + diff.Y);
                    _traceShader.Render(pos.X, pos.Y + diff.Y, pos.X, pos.Y);
                }

                _traceShader.End();
            }
            
            base.OnRenderFrame(e);

            _totalFrameTime += _frameTimer.ElapsedTicks;
            ++_framesCompleted;
            _frameTimer.Restart();
        }

        public override void OnMouseWheelChanged(MouseWheelEventArgs e)
        {
            if (e.DeltaPrecise >= 0f) {
                _camScale = Math.Min(8f / _upScale, _camScale * (1f + (e.DeltaPrecise / 4f)));
            } else {
                _camScale = Math.Max(1.6384f / _upScale, _camScale / (1f - (e.DeltaPrecise / 4f)));
            }
        }

        public override void OnMouseButtonDown(MouseButtonEventArgs e)
        {
            var pos = Camera.ScreenToWorld(new Vector2(e.X, e.Y) / _upScale, .5f);

            if (e.Button == MouseButton.Left) {
                _isSelecting = true;
                _selectionStart = pos;
            } else if (e.Button == MouseButton.Right) {
                foreach (var agent in World.Entities
                    .Where(x => x.HasComponent<Survivor>())
                    .Where(x => x.GetComponent<Survivor>().IsSelected)) {
                    agent.GetComponent<Survivor>().MovementCommand(pos);
                }
            }
        }

        public override void OnMouseButtonUp(MouseButtonEventArgs e)
        {
            var pos = Camera.ScreenToWorld(new Vector2(e.X, e.Y) / _upScale, .5f);

            if (e.Button == MouseButton.Left && _isSelecting) {
                var diff = World.Difference(pos, _selectionStart);

                _isSelecting = false;

                if (diff.LengthSquared < 0.25f) {
                    foreach (var ent in World.Entities) {
                        var human = ent.GetComponentOrNull<Human>();
                        if (human == null || !human.IsSelected) continue;

                        human.Deselect();
                    }

                    var best = new NearbyEntityEnumerator(World, pos, 2f)
                        .Where(x => x.HasComponent<Survivor>())
                        .OrderBy(x => World.Difference(pos, x.Position2D).LengthSquared)
                        .FirstOrDefault();

                    if (best != null) {
                        best.GetComponent<Human>().Select();
                    }
                } else {
                    if (diff.X < 0) {
                        var temp = _selectionStart.X;
                        _selectionStart.X = pos.X;
                        pos.X = temp;
                        diff.X = -diff.X;
                    }
                    
                    if (diff.Y < 0) {
                        var temp = _selectionStart.Y;
                        _selectionStart.Y = pos.Y;
                        pos.Y = temp;
                        diff.Y = -diff.Y;
                    }

                    foreach (var ent in World.Entities) {
                        var human = ent.GetComponentOrNull<Survivor>();
                        if (human == null || human.IsSelected || !human.Health.IsAlive) continue;

                        var edif = World.Difference(pos, ent.Position2D);

                        if (edif.X < 0 || edif.Y < 0 || edif.X > diff.X || edif.Y > diff.Y) continue;

                        human.Select();
                    }
                }
            }
        }

        public override void OnKeyPress(KeyboardKeyEventArgs e)
        {
            switch (e.Key) {
                case Key.Escape:
                    MainWindow.SetScene(_menu);
                    break;
                case Key.X:
                    _hideTop = !_hideTop;
                    break;
                case Key.P:
                    _drawPathNetwork = !_drawPathNetwork;
                    break;
                case Key.T:
                    _drawTestTrace = !_drawTestTrace;
                    break;
                case Key.Z:
                    _upScale = 3 - _upScale;
                    if (_upScale == 2) {
                        _camScale /= 2f;
                        Camera.Scale /= 2f;
                    } else {
                        _camScale *= 2f;
                        Camera.Scale *= 2f;
                    }
                    OnResize();
                    break;
                case Key.F11:
                    if (GameWindow.WindowState == WindowState.Fullscreen)
                        GameWindow.WindowState = WindowState.Normal;
                    else
                        GameWindow.WindowState = WindowState.Fullscreen;
                    break;
                case Key.G:
                    World = Generator.Generate(WorldSize, WorldSize);
                    Plugin.CityGenerated();
                    break;
            }

            base.OnKeyPress(e);
        }

        public override void Dispose()
        {
            World.Dispose();

            base.Dispose();
        }
    }
}
