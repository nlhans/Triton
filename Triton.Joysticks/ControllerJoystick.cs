using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using Timer = System.Timers.Timer;
using HANDLE = System.IntPtr;
using HWND = System.IntPtr;
using HDC = System.IntPtr;
using Microsoft.Win32;

namespace Triton.Joysticks
{
    public class JoystickDevice
    {
        private const string RegKeyAxisData = @"SYSTEM\ControlSet001\Control\MediaProperties\PrivateProperties\Joystick\OEM";
        private const string RegKeyPlace = @"System\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\";
        private const string RegReferencePlace = @"System\CurrentControlSet\Control\MediaResources\Joystick\DINPUT.DLL\CurrentJoystickSettings";

        public int id;
        public JOYCAPS data;

        private string _Name;
        private Dictionary<int, string> AxisNames = new Dictionary<int, string>();

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public JoystickDevice(JOYCAPS captured, int device)
        {
            id = device;

            // Copy all members.
            data = new JOYCAPS();
            data.szPname = captured.szPname;
            data.wMid = captured.wMid;
            data.wPid = captured.wPid;
            data.wXmin = captured.wXmin;
            data.wXmax = captured.wXmax;
            data.wYmin = captured.wYmin;
            data.wYmax = captured.wYmax;
            data.wZmin = captured.wZmin;
            data.wZmax = captured.wZmax;
            data.wNumButtons = captured.wNumButtons;
            data.wPeriodMin = captured.wPeriodMin;
            data.wPeriodMax = captured.wPeriodMax;


            RegistryKey rf = Registry.CurrentUser.OpenSubKey(RegReferencePlace);
            string USBDevice = Convert.ToString(rf.GetValue("Joystick" + (1 + id).ToString() + "OEMName"));
            RegistryKey usb = Registry.CurrentUser.OpenSubKey(RegKeyPlace);
            usb = usb.OpenSubKey(USBDevice);
            _Name = (string)usb.GetValue("OEMName");

            // Get axis stuff
            RegistryKey axisMaster = Registry.LocalMachine.OpenSubKey(RegKeyAxisData).OpenSubKey(USBDevice);
            
            AxisNames = new Dictionary<int, string>();
            if (axisMaster != null)
            {
                axisMaster = axisMaster.OpenSubKey("Axes");
                if (axisMaster != null)
                {
                    foreach (string name in axisMaster.GetSubKeyNames())
                    {
                        RegistryKey axis = axisMaster.OpenSubKey(name);
                        AxisNames.Add(Convert.ToInt32(name), (string)axis.GetValue(""));
                        axis.Close();
                    }
                    axisMaster.Close();
                }
            }
            rf.Close();
            usb.Close();
        }

        public static List<JoystickDevice> Search(string name)
        {
            List<JoystickDevice> results1 = JoystickDevice.Search();
            List<JoystickDevice> results2 = results1.FindAll (delegate(JoystickDevice dev) { return dev.Name.ToLower().Contains(name.ToLower()); });

            return results2;
        }


        /******************* STATIC ******************/
        static int deviceNumber = 0;
        public static List<JoystickDevice> Search()
        {
            List<JoystickDevice> Joysticks = new List<JoystickDevice>();

            JOYCAPS CapturedJoysticks;
            uint devs = JoystickMethods.joyGetNumDevs();
            for (deviceNumber = 0; deviceNumber < devs; deviceNumber++)
            {
                UInt32 res = JoystickMethods.joyGetDevCaps(deviceNumber, out CapturedJoysticks, JOYCAPS.Size);
                if (res != 165)
                {
                    Joysticks.Add(new JoystickDevice(CapturedJoysticks, deviceNumber));
                }
            }

            return Joysticks;
        }

    }

    public delegate void JoystickButtonPress(Joystick joystick, int button, bool state);
    public delegate void JoystickButtonEvent(Joystick joystick, int button);

    public class Joystick
    {
        private JoystickDevice device;
        private Timer _mJoystickUpdater;

        public JoystickButtonPress State;
        public JoystickButtonEvent Press;
        public JoystickButtonEvent Release;

        private List<bool> ButtonState = new List<bool>();
        private List<double> AxisState = new List<double>();

        private JOYINFOEX joyInfo;

        public void GetJoystickData()
        {
            joyInfo.dwSize = (Int32)Marshal.SizeOf(joyInfo);
            joyInfo.dwFlags = JoystickFlags.JOY_RETURNALL;
            JoystickMethods.joyGetPosEx(device.id, out joyInfo);
        }

        public Joystick(JoystickDevice dev)
        {
            device = dev;

            _mJoystickUpdater = new Timer();
            _mJoystickUpdater.Interval = 10;
            _mJoystickUpdater.Elapsed += _mJoystickUpdater_Tick;
            _mJoystickUpdater.Start();

            for (int i = 0; i < 6; i++)
                AxisState.Add(0);

        }

        void _mJoystickUpdater_Tick(object sender, EventArgs e)
        {
            GetJoystickData();

            AxisState[0] = joyInfo.dwXpos;
            AxisState[1] = joyInfo.dwYpos;
            AxisState[2] = joyInfo.dwZpos;
            AxisState[3] = joyInfo.dwRpos;
            AxisState[4] = joyInfo.dwUpos;
            AxisState[5] = joyInfo.dwVpos;

            for (int i = 0; i < 32; i++)
            {
                int bitmask = joyInfo.dwButtons & ((int)Math.Pow(2, i));
                if (ButtonState.Count <= i)
                {
                    ButtonState.Add((bitmask>0));
                    continue;
                }
                if (bitmask > 0)
                {
                    // Pressed
                    if (!ButtonState[i])
                    {
                        // EVENT press
                        if(State!=null)
                            State(this, i, true);
                        if (Press != null)
                            Press(this, i);

                    }
                    ButtonState[i] = true;
                }
                else
                {
                    if (ButtonState[i])
                    {
                        // EVENT release
                        if (State != null)
                            State(this, i, false);
                        if (Release !=null)
                        Release(this, i);
                    }
                    ButtonState[i] = false;
                }
            }
        }

        public double GetAxis(int id)
        {
            if (id < AxisState.Count)
                return AxisState[id];
            else
                return 0;
        }

        public bool GetButton(int id)
        {
            if (id < ButtonState.Count)
                return ButtonState[id];
            else
                return false;
        }

    }
}
