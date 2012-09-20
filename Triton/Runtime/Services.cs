using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel.Composition.Hosting;

namespace Triton.Runtime
{
    [Export(typeof(IServiceClass))]
    public class Services : IServiceClass
    {
        private static Dictionary<Assembly, ServicesInstance> _Services = new Dictionary<Assembly, ServicesInstance>();


        public static void RemoveServicesOf(Assembly source)
        {
            if (_Services.ContainsKey(source))
            {
                _Services[source].Kill();
                _Services.Remove(source);
            }
        }

        public static void BootServices()
        {
            Assembly source = Assembly.GetCallingAssembly();

            BootServicesOf(source);
        }

        public static void BootServicesOf(Assembly source)
        {
            BootServicesOf(source, true);
        }

        private static bool Exists(Assembly source)
        {
            foreach (KeyValuePair<Assembly, ServicesInstance> _mIn in _Services)
            {
                if (_mIn.Key.Location == source.Location)
                    return true;
            }
            return false;
        }

        private static void BootServicesOf(Assembly source, bool inter)
        {
            if (_Services.Count == 0 && inter) BootServicesOf(Assembly.GetAssembly(typeof(Services)), false);
            if (Exists(source)) return;

            ServicesInstance _mIn = new ServicesInstance(source);
            Debug.WriteLine("Starting services for " + source.Location);

            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(source));

            CompositionContainer container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(_mIn);
            container.Compose(batch);


            _Services.Add(source, _mIn);
            _Services[source].Boot();

        }

        public void Boot()
        {
            TritonBase.Exit += new AnonymousSignal(TritonBase_Exit);
        }

        void TritonBase_Exit()
        {
            RemoveServicesOf(Assembly.GetAssembly(typeof(Services)));
        }

        public void Kill()
        {
            Assembly _me = Assembly.GetAssembly(typeof(Services));
            while (_Services.Count > 0)
            {
                Assembly[] assems = new Assembly[_Services.Count];
                _Services.Keys.CopyTo(assems, 0);
                try
                {
                    if (assems[0] == _me && assems.Length == 1) break;
                    if (assems[0] == _me && assems.Length > 1)
                    {
                        RemoveServicesOf(assems[1]);
                    }
                    else
                    {
                        RemoveServicesOf(assems[0]);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                }
            }
        }

        public void Dispose()
        {

        }
    }
}
