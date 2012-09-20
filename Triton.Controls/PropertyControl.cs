using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Triton.Controls
{

    public class PropertyControl : UserControl
    {
        public const int LineHeight = 26;

        private Label _mL;

        public event AnonymousSignal Enter;
        public event AnonymousSignal ValueChanged;
        public string Text;

        public string Name;
        public PropertyType Type;
        private string _strValue;
        public int BoundsMin;
        public int BoundsMax;
        public int BoundsStep;
        public Dictionary<int, string> Selections = new Dictionary<int, string>();

        private bool CheckboxCheck = false;
        private int SelectedKey = 0;
        private int SelectedNumber = 0;

        public object Value
        {
            get
            {
                switch(Type)
                {
                    case PropertyType.StaticText:
                        return _strValue;
                        break;

                        case PropertyType.Checkbox:
                        CheckboxCheck = ck.Checked;
                        return ck.Checked;
                        break;

                        case PropertyType.Number:
                        SelectedNumber = Convert.ToInt32(nud.Value);
                        return nud.Value;
                        break;

                    case PropertyType.Selection:
                        // convert to int
                        foreach(KeyValuePair<int, string> KVP in Selections)
                        {
                            if(KVP.Value == cb.SelectedText)
                            {
                                SelectedKey = KVP.Key;
                                return KVP.Key;
                            }
                        }
                        return -1;
                        break;

                }

                return null;

            }
            set
            {
                switch(Type)
                {
                    case PropertyType.StaticText:
                        _strValue = (string)value;
                        this.Invoke(new AnonymousSignal(UpdateStaticText), new object[0]);
                        break;
                        
                    default:
                        _strValue = value.ToString();
                        break;

                }
            }
        }
        private void UpdateStaticText()
        {
            l.Text = _strValue;

        }

        public PropertyControl(string name, PropertyType type, string text, object initialvalue)
        {
            Name = name;
            Text = text;
            Type = type;
            _strValue = initialvalue.ToString();

            _mL = new Label();

            // Only draw when I am added!
            this.Controls.Add(_mL);

        }

        public void SetCursor(object o)
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new Signal(SetCursor), new object[1] {o});
                return;
            }
            bool on = (bool) o;

            if (on)
            {
                this.BackColor = SystemColors.ControlDark;
            }else
            {
                this.BackColor = SystemColors.Control;
            }
        }
        private
                    Label l = new Label();
        private NumericUpDown nud;
        private ComboBox cb;
        private CheckBox ck;
        public void Draw(Size s)
        {
            this.Size = new Size(s.Width - 10, LineHeight);

            int labelwidth = Convert.ToInt32(Math.Max(40, 0.4*this.Size.Width))-10;
            int boxwidth = this.Size.Width - labelwidth-10;
            Controls.Clear();

            _mL = new Label();
            _mL.Text = Text;
            _mL.Location = new Point(3, 6);
            _mL.Size = new Size(labelwidth, 34);
            Controls.Add(_mL);
            labelwidth += 5;
            switch(Type)
            {
                case PropertyType.StaticText:
                    l.Text = _strValue;
                    l.Size = new Size(boxwidth, LineHeight);
                    l.Location = new Point(labelwidth, 3);
                    Controls.Add(l);
                    break;

                case PropertyType.Number:
                    nud = new NumericUpDown();
                    nud.Maximum = BoundsMax;
                    nud.Minimum = BoundsMin;
                    nud.Increment = BoundsStep;
                    nud.Size = new Size(boxwidth, LineHeight);
                    nud.Location = new Point(labelwidth, 3);
                    int r = 0;
                    Int32.TryParse(_strValue, out r);
                    nud.Value = r;
                    Controls.Add(nud);
                    break;

                case PropertyType.Checkbox:

                    ck = new CheckBox();
                    bool c = false;
                    Boolean.TryParse(_strValue, out c);
                    ck.Checked = c;
                    ck.Size = new Size(boxwidth, LineHeight);
                    ck.Location = new Point(labelwidth, 3);
                    Controls.Add(ck);
                    break;

                    case PropertyType.Selection:
                    cb = new ComboBox();

                    foreach(KeyValuePair<int, string> kvp in Selections)
                    {
                        cb.Items.Add(kvp.Value);
                    }


                    cb.Size = new Size(boxwidth, LineHeight);
                    cb.Location = new Point(labelwidth, 3);
                    Controls.Add(cb);
                    break;
            }
        }
        //TOOD: ADD KEYBOARD
        public void ButtonInc()
        {
            switch(Type)
            {
                case PropertyType.Number:
                    if (nud.Value != nud.Maximum)
                    {
                        nud.Invoke(new AnonymousSignal(nud.UpButton), new object[0]);
                        FireValueChanged();
                    }
            

            break;

                case PropertyType.Text:
                    // SHOW KEYBOARD
                    break;
            }

        }
        public void ButtonDec()
        {

            switch (Type)
            {
                case PropertyType.Number:
                    if (nud.Value != nud.Minimum)
                    {
                        nud.Invoke(new AnonymousSignal(nud.DownButton), new object[0]);
                        FireValueChanged();
                    }
                    break;
                case PropertyType.Text:
                    // HIDE KEYBOARD
                    break;
            }


        }

        public bool ButtonDown()
        {
            switch (Type)
            {
                case PropertyType.Selection:
                    if (cb.InvokeRequired)
                        return Convert.ToBoolean(cb.Invoke(new AnonymousFeedbackSignal(ButtonDown), new object[0]));
                    if (cb.DroppedDown)
                    {
                        if(cb.SelectedIndex+1 < cb.Items.Count)
                        cb.SelectedIndex++;
                        return false;
                    }
                    break;
            }
            return true;

        }
        public bool ButtonUp()
        {
            switch (Type)
            {
                case PropertyType.Selection:
                    if (cb.InvokeRequired)
                        return Convert.ToBoolean(cb.Invoke(new AnonymousFeedbackSignal(ButtonUp), new object[0]));
                    if (cb.DroppedDown)
                    {
                        if(cb.SelectedIndex > 0)
                        cb.SelectedIndex--;
                        return false;
                    }
                    break;
            }
            return true;

        }

        private void ToggleCK()
        {
            ck.Checked = !ck.Checked;

        }

        public void ButtonEnter()
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new AnonymousSignal(ButtonEnter), new object[0]);
                return;
            }
            switch (Type)
            {
                case PropertyType.Selection:
                    cb.DroppedDown = !cb.DroppedDown;
                    if (cb.DroppedDown == false)
                        FireValueChanged();
                    break;

                case PropertyType.Text:
                    // SHOW KEYBOARD
                    break;
                    case PropertyType.Checkbox:
                    ck.Invoke(new AnonymousSignal(ToggleCK), new object[0] { });
                        FireValueChanged();
                    break;

                    default:
                    FireValueChanged();
                    break;

            }

            if (Enter != null) Enter();
        }

        private void FireValueChanged()
        {
            if (ValueChanged != null)
                ValueChanged();
        }
    }
}