using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zombles.UI
{
    public class UINumericUpDown : UIObject
    {
        private UIButton _minusButton;
        private UIButton _plusButton;
        private UINumericTextBox _textEntry;

        public int Minimum { get { return _textEntry.Minimum; } set { _textEntry.Minimum = value; } }

        public int Maximum { get { return _textEntry.Maximum; } set { _textEntry.Maximum = value; } }

        public int Value { get { return _textEntry.Value; } set { _textEntry.Value = value; } }
    }
}
