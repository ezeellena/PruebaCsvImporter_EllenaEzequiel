using ConsoleAppWithDI.UI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace testDI
{
    [TestClass]
    public class UnitTest1 : DbContext
    {
        private IConfigurationRoot _configuration;
        private DbContextOptions<AcmeCorporationContext> _options;
        [TestInitialize]
        public void iniciar()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
            _options = new DbContextOptionsBuilder<AcmeCorporationContext>()
                .UseSqlServer(_configuration.GetConnectionString("AcmeCorporation"))
                .Options;
        }
       
        [TestMethod]
        public void TestMethod1()
        {

        }
    }
}
