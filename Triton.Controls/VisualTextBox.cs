using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Drawing;

namespace Triton.Controls
{
    public enum VisualTextType
    {
        DigitsOnly,
        All
    }

    public class VisualTextBox : TextBox
    {
        public event EventHandler EnterPressed;

        private VisualTextType _Type = VisualTextType.All;
        public VisualTextType Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
            }

        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (_Type == VisualTextType.DigitsOnly)
            {
                if (Char.IsDigit(e.KeyChar) == false)
                    e.Handled = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs kevent)
        {
            base.OnKeyUp(kevent);
            if (EnterPressed != null && kevent.KeyCode == Keys.Enter)
                EnterPressed(this, new EventArgs());
        }
    }
}
