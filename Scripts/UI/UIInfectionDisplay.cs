using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;

using Zombles.Graphics;
using Zombles.UI;

using Zombles.Entities;
using Zombles.Scripts.Entities;
using Zombles.Geometry;

namespace Zombles.Scripts.UI
{
    public class UIInfectionDisplay : UIObject
    {
        private City _city;

        private UISprite _survivorBar;
        private UISprite _zombieBar;

        public UIInfectionDisplay(City city)
        {
            _city = city;

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

            int survivors = _city.Entities.Where(x => x.HasComponent<Survivor>())
                .Count(x => x.GetComponent<Health>().IsAlive);
            
            int zombies = _city.Entities.Where(x => x.HasComponent<Zombie>())
                .Count(x => x.GetComponent<Health>().IsAlive);

            float ratio = (float) survivors / (zombies + survivors);

            _survivorBar.Width = _zombieBar.Left = (int) Math.Round(InnerWidth * ratio);
            _zombieBar.Width = InnerWidth - _survivorBar.Width;
        }
    }
}
