using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Triton.Joysticks;

namespace Triton.Controls
{
    public class PropertyBox : Panel
    {
        private Joystick g25;
        public List<PropertyControl> Properties = new List<PropertyControl>();
        private int cursor = 0;
        private Thread _mButtonRepeater;
        public PropertyBox()
        {
            // Listen to G25
            g25 = new Joystick(JoystickDevice.Search("G25")[0]);

            g25.Press += new JoystickButtonEvent(g25_Press);
            g25.Release += new JoystickButtonEvent(g25_Release);

            _mButtonRepeater = new Thread(ButtonRepeat);
            _mButtonRepeater.IsBackground = true;
            _mButtonRepeater.Start();
        }
        private void ButtonRepeat()
        {
            while (true)
            {
                try
                {
                    while (ButtonsHolding.Count == 0)
                        Thread.Sleep(new TimeSpan(10000)); // 100 us
                    int iterations = 0;
                    while (ButtonsHolding.Count > 0)
                    {
                        if (iterations == 0)
                            Thread.Sleep(500);
                        if (ButtonsHolding.Contains(128)) ButtonsHolding.Remove(128);
                        if (ButtonsHolding.Contains(3)) ButtonsHolding.Remove(3); // dont repeat ENTER
                        lock (ButtonsHolding)
                        {
                            foreach (int b in ButtonsHolding)
                            {
                                Press(b);
                            }
                        }
                        iterations++;
                        if (iterations > 3)
                            Thread.Sleep(150);
                        else
                            Thread.Sleep(650);

                    }
                }
                catch (Exception ex)
                {
                    //
                }

            }
        }

        void g25_Release(Joystick joystick, int button)
        {
            ButtonsHolding.Remove(button);
        }

        private void
                UpdateCursor()
        {
            int i = 0;
            foreach (PropertyControl p in Properties)
            {
                p.SetCursor(i == cursor);
                i++;
            }
        }
        private void
                UpdateValueUp()
        {
            Properties[cursor].ButtonInc();
        }
        private void
                UpdateValueDown()
        {
            Properties[cursor].ButtonDec();
        }
        private void UpdateValueEnter()
        {
            Properties[cursor].ButtonEnter();
        }

        private List<int> ButtonsHolding = new List<int>();

        void g25_Press(Joystick joystick, int button)
        {
            ButtonsHolding.Add(button);

            Press(button);
        }
        private void Press(int button)
        {

            if (cursor >= Properties.Count)
                cursor = Properties.Count - 1;
            // Viewhat 1 (REAL ONE)
            if (button == 129)
            {
                //up
                if (Properties[cursor].ButtonUp())
                {
                    cursor--;
                    if (cursor < 0)
                        cursor = 0;
                    if (cursor >= Properties.Count)
                        cursor = Properties.Count - 1;
                    UpdateCursor();
                }
            }
            if (button == 131)
            {
                // down
                if (Properties[cursor].ButtonDown())
                {
                    cursor++;
                    if (cursor >= Properties.Count)
                        cursor = Properties.Count - 1;
                    UpdateCursor();
                }
            }
            if (button == 132)
            {
                //l eft
                UpdateValueDown();
            }
            if (button == 130)
            {
                //right
                UpdateValueUp();
            }

            // Viewhat 2
            if (button == 16)
            {
                // left

            }
            if (button == 18)
            {
                // right

            }
            if (button == 15)
            {
                // top

            }
            if (button == 17)
            {
                // down

            }

            // Flippers
            if (button == 5)
            {
                // LEFT FLIPPER 
            }
            if (button == 4)
            {
                // LEFT FLIPPER 
            }

            // Stuurwiel knopjes

            if (button == 7)
            {
                // LEFT  
            }
            if (button == 6)
            {
                // LEFT  
            }

            // Button rij!

            if (button == 0)
            {
                // LEFT  
            }
            if (button == 1)
            {
                // LEFT MID 
            }
            if (button == 2)
            {
                // RIGHT MID
            }
            if (button == 3)
            {
                // RIGHT
                UpdateValueEnter();
            }
        }

        public void UpdateForm()
        {
            Controls.Clear();
            int i = 0;
            foreach (PropertyControl c in Properties)
            {
                c.Draw(this.Size);
                c.Location = new Point(0, i * PropertyControl.LineHeight);
                Controls.Add(c);
                i++;
            }
            UpdateCursor();
        }
    }
}