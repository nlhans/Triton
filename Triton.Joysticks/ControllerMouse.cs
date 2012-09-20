using System;
using System.Collections.Generic;
using System.Text;

namespace Triton.Joysticks
{
    /*
     * TODO: ADd support for moouse controllers
     * Hardware controllers?
     */
    /*
    public class Mouse : IController
    {
        public string Type { get { return "Mouse"; } }

        public double Speed
        {
            get
            {
                double MouseY = System.Windows.Forms.Cursor.Position.Y;
                double ScreenY = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

                return (MouseY / ScreenY * -2 + 1) ;
            }
        }

        public double SteerAngle
        {
            get
            {
                double MouseX = System.Windows.Forms.Cursor.Position.X;
                double ScreenX = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;

                return ( MouseX/ScreenX * 2 - 1);

            }
        }
        
    }
     */
}
