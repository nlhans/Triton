using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Triton.Debugging
{
    public abstract class UnitTestable : IUnitTestable
    {
        public UnitTestable()
        {

        }

        protected void _Register(object overrider)
        {
            UnitTester.Register(overrider);

        }

        internal virtual void _ExecuteTests()
        {

        }
    }
}
