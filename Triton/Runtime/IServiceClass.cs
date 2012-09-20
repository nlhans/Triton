using System;

namespace Triton.Runtime
{
    public interface IServiceClass : IDisposable
    {
        void Boot();
        void Kill();
    }
}