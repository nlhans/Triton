using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Triton
{
    public class TritonBase
    {
        private static bool _Active = true;
        public static bool Active
        {
            get
            {
                return _Active;
            }

        }
        internal static event AnonymousSignal Close;
        public static event AnonymousSignal Exit;
        public static event AnonymousSignal PreExit;
        public static event AnonymousSignal AfterExit;
        public static void TriggerExit()
        {
            _Active = false;
            if (PreExit != null)
                PreExit();

            if (Close != null)
                Close();
            if (Exit != null)
                Exit();

            if (AfterExit != null)
                AfterExit();
        }
    }
}
