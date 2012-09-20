using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reflection;
using Triton.Debugging;

namespace Triton.Runtime
{
    public class ServicesInstance
    {
        private Assembly _mAss;
        public Assembly Assembly
        {
            get { return _mAss; }
        }
        [ImportMany(typeof(IServiceClass))]
        private IEnumerable<IServiceClass> _mServices;
        public IEnumerable<IServiceClass> Services
        {
            get { return _mServices; }
            set { _mServices = value; }

        }
        public ServicesInstance(Assembly assem)
        {
            _mAss = assem;

        }

        public void Boot()
        {
            if (_mServices != null)
            {
                foreach (IServiceClass cl in _mServices)
                {
                    cl.Boot();

                }
            }
        }

        public void Kill()
        {
            Debug.WriteLine("Killing services for " + _mAss.Location);
            if (_mServices != null)
            {
                foreach (IServiceClass cl in _mServices)
                {
                    try
                    {
                        cl.Kill();
                        cl.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine(ex.StackTrace);
                        Logger.Log(this, ex.Message);
                        Logger.Log(this, ex.StackTrace);
                    }

                }
            }
        }
    }
}