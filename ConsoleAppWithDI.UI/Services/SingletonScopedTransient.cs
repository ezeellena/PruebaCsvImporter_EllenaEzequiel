using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleAppWithDI.UI.Services
{
    public class SingletonScopedTransient : ITransientService, IScopedService, ISingletonService
    {
        Guid id;
        public SingletonScopedTransient()
        {
            id = Guid.NewGuid();
        }
        public Guid getGuid()
        {
            return id;
        }
    }
}
