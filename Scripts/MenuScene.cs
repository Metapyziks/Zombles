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

        private UIButton _continueBtn;

        public MenuScene(MainWindow game)
            : base(game)
        {

        }

        public override void OnEnter(bool firstTime)
        {
            base.OnEnter(firstTime);

            if (firstTime) {
                var worldSizeLbl = new UILabel(Graphics.PixelFont.Large, new Vector2(36, 36)) {
                    Text = "World Size",
                    Colour = Color4.White
                };

                AddChild(worldSizeLbl);

                var worldSizeTxt = new UINumericTextBox(new Vector2(48, 20), new Vector2(32 + 152, 32)) {
                    Minimum = 32,
                    Maximum = 256,
                    Value = 64
                };

                worldSizeTxt.UnFocused += (sender, e) => {
                    Program.WorldSize = worldSizeTxt.Value;
                };

                AddChild(worldSizeTxt);

                var humanCountLbl = new UILabel(Graphics.PixelFont.Large, new Vector2(36, worldSizeTxt.Bottom + 12)) {
                    Text = "Survivor Count",
                    Colour = Color4.White
                };

                AddChild(humanCountLbl);

                var humanCountTxt = new UINumericTextBox(new Vector2(48, 20), new Vector2(32 + 152, worldSizeTxt.Bottom + 8)) {
                    Minimum = 0,
                    Maximum = 512,
                    Value = 96
                };

                humanCountTxt.UnFocused += (sender, e) => {
                    Program.SurvivorCount = humanCountTxt.Value;
                };

                AddChild(humanCountTxt);

                var zombieCountLbl = new UILabel(Graphics.PixelFont.Large, new Vector2(36, humanCountTxt.Bottom + 12)) {
                    Text = "Zombie Count",
                    Colour = Color4.White
                };

                AddChild(zombieCountLbl);

                var zombieCountTxt = new UINumericTextBox(new Vector2(48, 20), new Vector2(32 + 152, humanCountTxt.Bottom + 8)) {
                    Minimum = 0,
                    Maximum = 512,
                    Value = 32
                };

                zombieCountTxt.UnFocused += (sender, e) => {
                    Program.ZombieCount = zombieCountTxt.Value;
                };

                AddChild(zombieCountTxt);

                var generateBtn = new UIButton(new Vector2(96, 32), new Vector2(32, zombieCountTxt.Bottom + 8)) {
                    Text = "Start New",
                    CentreText = true
                };

                generateBtn.Click += (sender, e) => {
                    if (_gameScene != null) _gameScene.Dispose();

                    _gameScene = new GameScene(GameWindow, this);
                    MainWindow.SetScene(_gameScene);
                };

                AddChild(generateBtn);

                _continueBtn = new UIButton(generateBtn.Size, generateBtn.Position + new Vector2(generateBtn.Width + 8, 0)) {
                    Text = "Continue",
                    CentreText = true,
                    IsEnabled = false
                };

                _continueBtn.Click += (sender, e) => {
                    MainWindow.SetScene(_gameScene);
                };

                AddChild(_continueBtn);
            }
            
            _continueBtn.IsEnabled = _gameScene != null;
        }
    }
}
