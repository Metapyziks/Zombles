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
        private UISprite mySurvivorBar;
        private UISprite myZombieBar;

        public UIInfectionDisplay()
        {
            mySurvivorBar = new UISprite( new Sprite( 0.5f, 1.0f, Color4.Pink ) );
            myZombieBar = new UISprite( new Sprite( 0.5f, 1.0f, Color4.LightGreen ) );
            myZombieBar.Left = 0.5f;

            AddChild( mySurvivorBar );
            AddChild( myZombieBar );

            UpdateBars();
        }

        protected override Vector2 OnSetSize( Vector2 newSize )
        {
            UpdateBars();

            return newSize;
        }

        public void UpdateBars()
        {
            mySurvivorBar.Height = myZombieBar.Height = InnerHeight;

            float ratio = (float) Survivor.Count / ( Zombie.Count + Survivor.Count );

            mySurvivorBar.Width = myZombieBar.Left = (int) Math.Round( InnerWidth * ratio );
            myZombieBar.Width = InnerWidth - mySurvivorBar.Width;
        }
    }
}
