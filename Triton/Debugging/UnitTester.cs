using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Triton.Debugging
{
    public class UnitTester
    {
        private static Dictionary<Type, UnitTestable> Tests = new Dictionary<Type, UnitTestable>();
#if DEBUG
        public static void Register(object testable)
        {
            //
            Type typ = testable.GetType();

            if (typ.IsSubclassOf(typeof(UnitTestable)))
                Logger.Log(typeof(UnitTester), "You should register only UnitTestable objects!");

            if (Tests.ContainsKey(typ))
                Logger.Log(typeof(UnitTester), "Could not register unittest " + typ.Name + ", already added!");

            Tests.Add(typ, (UnitTestable)testable);
        }
#else
        public static void Register(object testable)
        {
        }
#endif
        public static void Assert(bool condition)
        {
            if (!condition)
            {


            }
        }

        public static void Test(string Test)
        {

        }

        public static void Run()
        {
            //

        }

        public static void Run(Type cls)
        {

        }
    }
}
