using System;

namespace Triton.Maths
{
    public static class IntegerMath
    {
        public static bool InRange(int val, int min, int max)
        {
            if (val >= min && val <= max)
                return true;
            return false;
        }
        public static int InRange(int val, int min, int max, int ret)
        {
            if (val >= min && val <= max)
                return ret;
            return 0;
        }
    }
}
