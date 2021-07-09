using ConsoleAppWithDI.UI.Services;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleAppWithDI.UI
{
    public class DI 
    {
        private readonly ILogger<DI> _logger;
        private readonly ITransientService _transientService1;
        private readonly ITransientService _transientService2;
        private readonly IScopedService _scopedService1;
        private readonly IScopedService _scopedService2;
        private readonly ISingletonService _singletonService1;
        private readonly ISingletonService _singletonService2;
        public DI(
            ISingletonService singleton,
            ISingletonService singleton2,

            IScopedService Scoped,
            IScopedService Scoped2,

            ITransientService Transient,
            ITransientService Transient2,
            ILogger<DI> logger

            )
        {
            _singletonService1 = singleton;
            _singletonService2 = singleton2;

            _scopedService1 = Scoped;
            _scopedService2 = Scoped2;

            _transientService1 = Transient;
            _transientService2 = Transient2;
            _logger = logger;
        }
        public void Run()
        {
            var singleton = _singletonService1.getGuid().ToString();
            _logger.LogError(singleton);
            
            var singleton2 = _singletonService2.getGuid().ToString();
            _logger.LogError(singleton2);

            var Scoped = _scopedService1.getGuid().ToString();
            _logger.LogError(Scoped);

            var Scoped2 = _scopedService2.getGuid().ToString();
            _logger.LogError(Scoped2);

            var Transient = _transientService1.getGuid().ToString();
            _logger.LogError(Transient);

            var Transient2 = _transientService2.getGuid().ToString();
            _logger.LogError(Transient2);
        }
    }
}
