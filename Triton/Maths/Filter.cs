using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Triton.Maths
{
    public interface IFilterMethod
    {
        double Filter(List<double> f);
    }

    public class Filter
    {
        public int MaxSize
        {
            get
            {
                return _MaxSize;
            }
            set
            {
                _MaxSize = value;
            }
        }
        private int _MaxSize = 1000;
        public List<double> Values
        {
            get
            {
                return _Values;
            }
            set
            {
                _Values = value;
            }
        }
        private List<double> _Values = new List<double>();

        private class AverageFilter : IFilterMethod
        {
            public double Filter(List<double> values)
            {
                double sum = 0;
                foreach (double v in values) sum += v;
                return (sum / values.Count);
            }
        }

        private static AverageFilter _fAvg = new AverageFilter();

        public double Average
        {
            get
            {
                return _fAvg.Filter(_Values);
            }
        }

        public double Apply(IFilterMethod filter)
        {
            double result = filter.Filter(_Values);
            return result;
        }

        public void Clear()
        {
            this.Values.Clear();

        }

        public void Add(double sample)
        {
            if (_MaxSize < 2) _MaxSize = 2;
            while (Values.Count >= _MaxSize)
                Values.RemoveAt(0);
            Values.Add(sample);

        }

        public Filter()
        {

        }

        public Filter(int size)
        {
            _MaxSize = size;
        }

    }
}
