using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Triton
{
    public delegate bool AnonymousFeedbackSignal();
    public delegate void AnonymousSignal();
    public delegate void Signal(object sender);

}
