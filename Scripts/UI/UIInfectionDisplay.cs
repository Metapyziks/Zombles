using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;

using Zombles.Graphics;
using Zombles.UI;

using Zombles.Scripts.Entities;

namespace Zombles.Scripts.UI
{
    public class UIInfectionDisplay : UIObject
    {
        private UISprite _survivorBar;
        private UISprite _zombieBar;

        public UIInfectionDisplay()
        {
            _survivorBar = new UISprite(new Sprite(0.5f, 1.0f, Color4.Pink));
            _zombieBar = new UISprite(new Sprite(0.5f, 1.0f, Color4.LightGreen));
            _zombieBar.Left = 0.5f;

            AddChild(_survivorBar);
            AddChild(_zombieBar);

            UpdateBars();
        }

        protected override Vector2 OnSetSize(Vector2 newSize)
        {
            UpdateBars();

            return newSize;
        }

        public void UpdateBars()
        {
            _survivorBar.Height = _zombieBar.Height = InnerHeight;

            float ratio = (float) Survivor.Count / (Zombie.Count + Survivor.Count);

            _survivorBar.Width = _zombieBar.Left = (int) Math.Round(InnerWidth * ratio);
            _zombieBar.Width = InnerWidth - _survivorBar.Width;
        }
    }
}
