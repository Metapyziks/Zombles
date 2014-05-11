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

        private UIButton _aiTypeBtn;
        private UIButton _playerControlBtn;
        private UIButton _generateBtn;
        private UIButton _quitBtn;
        private UIButton _continueBtn;

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
                + _aiTypeBtn.Height + 8
                + _generateBtn.Height + 8
                + _continueBtn.Height;

            var origin = new Vector2((int) ((Width - width) / 2f), (int) ((Height - height) / 2f));

            _worldSizeLbl.Position = origin + new Vector2(4, 4);
            _worldSizeTxt.Position = origin + new Vector2(152, 0);

            _humanCountLbl.Position = _worldSizeLbl.Position + new Vector2(0, _worldSizeTxt.Height + 8);
            _humanCountTxt.Position = _worldSizeTxt.Position + new Vector2(0, _worldSizeTxt.Height + 8);

            _zombieCountLbl.Position = _humanCountLbl.Position + new Vector2(0, _humanCountTxt.Height + 8);
            _zombieCountTxt.Position = _humanCountTxt.Position + new Vector2(0, _humanCountTxt.Height + 8);

            _aiTypeBtn.Position = new Vector2(origin.X, _zombieCountTxt.Bottom + 8);
            _playerControlBtn.Position = new Vector2(_aiTypeBtn.Right + 8, _aiTypeBtn.Top);

            _generateBtn.Position = new Vector2(_aiTypeBtn.Left, _aiTypeBtn.Bottom + 8);
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
                    Value = Program.WorldSize
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
                    Value = Program.SurvivorCount
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
                    Value = Program.ZombieCount
                };

                AddChild(_zombieCountTxt);

                var aiTypes = new String[] {
                    "Basic AI",
                    "Subsumpt",
                    "Fast BDI",
                    "Slow BDI"
                };

                _aiTypeBtn = new UIButton(new Vector2(96, 32)) {
                    Text = aiTypes[0],
                    CentreText = true
                };

                _aiTypeBtn.Click += (sender, e) => {
                    int index = Array.IndexOf(aiTypes, _aiTypeBtn.Text);
                    index = (index + 1) % aiTypes.Length;
                    _aiTypeBtn.Text = aiTypes[index];

                    switch (aiTypes[index]) {
                        case "Basic AI":
                            Program.Subsumption = false;
                            Program.Deliberative = false;
                            break;
                        case "Subsumpt":
                            Program.Subsumption = true;
                            Program.Deliberative = false;
                            break;
                        case "Fast BDI":
                            Program.Subsumption = false;
                            Program.Deliberative = true;
                            Program.FastDeliberative = true;
                            break;
                        case "Slow BDI":
                            Program.Subsumption = false;
                            Program.Deliberative = true;
                            Program.FastDeliberative = false;
                            break;
                    }

                    _playerControlBtn.IsEnabled = aiTypes[index] != "Basic AI";

                    if (!_playerControlBtn.IsEnabled) {
                        _playerControlBtn.Text = "Autonomous";
                        Program.PlayerControl = false;
                    }
                };

                AddChild(_aiTypeBtn);

                _playerControlBtn = new UIButton(new Vector2(96, 32)) {
                    Text = "Autonomous",
                    CentreText = true,
                    IsEnabled = false
                };

                _playerControlBtn.Click += (sender, e) => {
                    Program.PlayerControl = !Program.PlayerControl;
                    if (Program.PlayerControl) {
                        _playerControlBtn.Text = "Interactive";
                    } else {
                        _playerControlBtn.Text = "Autonomous";
                    }
                };

                AddChild(_playerControlBtn);

                _generateBtn = new UIButton(_aiTypeBtn.Size) {
                    Text = "Start New",
                    CentreText = true
                };

                _generateBtn.Click += (sender, e) => {
                    if (_gameScene != null) _gameScene.Dispose();

                    Program.Seed = (int) (DateTime.Now.Ticks % int.MaxValue) + 1;

                    Program.WorldSize = _worldSizeTxt.Value;
                    Program.SurvivorCount = _humanCountTxt.Value;
                    Program.ZombieCount = _zombieCountTxt.Value;
                    Program.PlayerControl = _playerControlBtn.Text == "Interactive";

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
