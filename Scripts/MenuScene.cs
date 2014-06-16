using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using Zombles.UI;

namespace Zombles.Scripts
{
    public class MenuScene : Scene
    {
        private GameScene _gameScene;

        private UIPanel _filter;
        
        private UILabel _worldSizeLbl;
        private UINumericTextBox _worldSizeTxt;

        private UILabel _humanCountLbl;
        private UINumericTextBox _humanCountTxt;

        private UILabel _zombieCountLbl;
        private UINumericTextBox _zombieCountTxt;

        private UIButton _generateBtn;
        private UIButton _quitBtn;
        private UIButton _continueBtn;

        public int WorldSize { get { return _worldSizeTxt.Value; } }

        public int HumanCount { get { return _humanCountTxt.Value; } }

        public int ZombieCount { get { return _zombieCountTxt.Value; } }

        public MenuScene(MainWindow game)
            : base(game) { }

        public override void OnResize()
        {
            RepositionElements();

            base.OnResize();

            if (_gameScene != null) _gameScene.OnResize();
        }

        private void RepositionElements()
        {
            _filter.Size = new Vector2(Width, Height);

            float width = _continueBtn.Width;
            float height = _worldSizeTxt.Height + 8
                + _humanCountTxt.Height + 8
                + _zombieCountTxt.Height + 8
                + _generateBtn.Height + 8
                + _continueBtn.Height;

            var origin = new Vector2((int) ((Width - width) / 2f), (int) ((Height - height) / 2f));

            _worldSizeLbl.Position = origin + new Vector2(4, 4);
            _worldSizeTxt.Position = origin + new Vector2(152, 0);

            _humanCountLbl.Position = _worldSizeLbl.Position + new Vector2(0, _worldSizeTxt.Height + 8);
            _humanCountTxt.Position = _worldSizeTxt.Position + new Vector2(0, _worldSizeTxt.Height + 8);

            _zombieCountLbl.Position = _humanCountLbl.Position + new Vector2(0, _humanCountTxt.Height + 8);
            _zombieCountTxt.Position = _humanCountTxt.Position + new Vector2(0, _humanCountTxt.Height + 8);

            _generateBtn.Position = new Vector2(origin.X, _zombieCountTxt.Bottom + 8);
            _quitBtn.Position = new Vector2(_generateBtn.Right + 8, _generateBtn.Top);
            _continueBtn.Position = _generateBtn.Position + new Vector2(0, _generateBtn.Height + 8);
        }

        public override void OnKeyPress(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.F11) {
                if (GameWindow.WindowState == WindowState.Fullscreen)
                    GameWindow.WindowState = WindowState.Normal;
                else
                    GameWindow.WindowState = WindowState.Fullscreen;
                return;
            }

            base.OnKeyPress(e);
        }

        public override void OnEnter(bool firstTime)
        {
            base.OnEnter(firstTime);

            MainWindow.IsPaused = true;

            if (firstTime) {
                _filter = new UIPanel(new Vector2(Width, Height)) {
                    Colour = new Color4(0, 0, 0, 191)
                };

                AddChild(_filter);

                _worldSizeLbl = new UILabel(Graphics.PixelFont.Large) {
                    Text = "World Size",
                    Colour = Color4.White
                };

                AddChild(_worldSizeLbl);

                _worldSizeTxt = new UINumericTextBox(new Vector2(48, 20)) {
                    Minimum = 32,
                    Maximum = 256,
                    Value = 128
                };

                AddChild(_worldSizeTxt);

                _humanCountLbl = new UILabel(Graphics.PixelFont.Large) {
                    Text = "Survivor Count",
                    Colour = Color4.White
                };

                AddChild(_humanCountLbl);

                _humanCountTxt = new UINumericTextBox(new Vector2(48, 20)) {
                    Minimum = 0,
                    Maximum = 512,
                    Value = 192
                };

                AddChild(_humanCountTxt);

                _zombieCountLbl = new UILabel(Graphics.PixelFont.Large) {
                    Text = "Zombie Count",
                    Colour = Color4.White
                };

                AddChild(_zombieCountLbl);

                _zombieCountTxt = new UINumericTextBox(new Vector2(48, 20)) {
                    Minimum = 0,
                    Maximum = 512,
                    Value = 64
                };

                AddChild(_zombieCountTxt);

                _generateBtn = new UIButton(new Vector2(96, 32)) {
                    Text = "Start New",
                    CentreText = true
                };

                _generateBtn.Click += (sender, e) => {
                    if (_gameScene != null) _gameScene.Dispose();

                    _gameScene = new GameScene(GameWindow, this);
                    MainWindow.SetScene(_gameScene);
                };

                AddChild(_generateBtn);

                _quitBtn = new UIButton(_generateBtn.Size) {
                    Text = "Quit",
                    CentreText = true
                };

                _quitBtn.Click += (sender, e) => {
                    GameWindow.Close();
                };

                AddChild(_quitBtn);

                _continueBtn = new UIButton(new Vector2(_quitBtn.Width + _generateBtn.Width + 8, _generateBtn.Height)) {
                    Text = "Continue",
                    CentreText = true,
                    IsEnabled = false
                };

                _continueBtn.Click += (sender, e) => {
                    MainWindow.SetScene(_gameScene);
                };

                AddChild(_continueBtn);
            }

            RepositionElements();
            
            _continueBtn.IsEnabled = _gameScene != null;
        }

        public override void OnExit()
        {
            base.OnExit();

            MainWindow.IsPaused = false;
        }

        public override void OnRenderFrame(FrameEventArgs e)
        {
            if (_gameScene != null) {
                _gameScene.OnRenderFrame(e);
            }

            base.OnRenderFrame(e);
        }
    }
}
