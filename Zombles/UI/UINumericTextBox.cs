using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Zombles.UI
{
    public class UINumericTextBox : UITextBox
    {
        public int Minimum { get; set; }

        public int Maximum { get; set; }

        public int Value { get { return int.Parse(Text); } set { Text = value.ToString(); } }

        public UINumericTextBox(float scale = 1.0f)
            : base(scale) { }

        public UINumericTextBox(Vector2 size, float scale = 1.0f)
            : base(size, scale) { }

        public UINumericTextBox(Vector2 size, Vector2 position, float scale = 1.0f)
            : base(size, position, scale) { }

        protected override bool OnValidateString(ref string str)
        {
            int val;

            return str.Length == 0 || (int.TryParse(str, out val) && val <= Maximum);
        }

        protected override void OnUnFocus()
        {
            base.OnUnFocus();

            if (Text.Length == 0) {
                Text = Minimum.ToString();
            }

            if (Value < Minimum) Value = Minimum;
            if (Value > Maximum) Value = Maximum;
        }
    }
}
