using System;
using System.Linq;
using CsvImporter.Models;
using CsvImporter.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CsvImporter
{
    public class Csv
    {
        private readonly IDescargaCsvService _descargaCsvService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Csv> _logger;
        private readonly AcmeCorporationContext _acmeCorporationContext;

        public Csv(IDescargaCsvService descargaCsvService, 
            IConfiguration configuration, 
            ILogger<Csv> logger,
            AcmeCorporationContext acmeCorporationContext)
        {
            this._descargaCsvService = descargaCsvService;
            this._configuration = configuration;
            this._logger = logger;
            this._acmeCorporationContext = acmeCorporationContext;
        }

        public void Run(String[] args)
        {
            //_acmeCorporationContext.Database.ExecuteSqlRaw("insert into Stock values (100,10000,getdate(),2)");
            if (_acmeCorporationContext.Stock.Count() > 0)
            {

                _logger.LogInformation("La Tabla tiene registros");
                _logger.LogInformation("Eliminando Registros");
                _acmeCorporationContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.Stock");
                _logger.LogInformation("Registros Eliminados Correctamente");

                DescargaEInsert();

                _logger.LogInformation("Terminado Correctamente");

            }
            else
            {
                DescargaEInsert();
            }

        }
        private void DescargaEInsert()
        {
            string directorioBase =  AppDomain.CurrentDomain.BaseDirectory;
            string urlcsv = _configuration.GetSection("Csv").Value;
            string CarpetaDescarga = _configuration.GetSection("Carpeta").Value;
            _logger.LogInformation("Descargando Csv " + urlcsv + " en la carpeta " + CarpetaDescarga + "");

            var resultado = _descargaCsvService.DescargarCsv(urlcsv, CarpetaDescarga, 0);

            _logger.LogInformation($"Archivo: {resultado.UbicacionArchivo}");
            _logger.LogInformation($"Tamaño: {resultado.Tamaño} bytes");
            _logger.LogInformation($"Tiempo Descarga: {resultado.TiempoDemora.Minutes} minutos");
            _logger.LogInformation($"Descargas paralelas: {resultado.DescargasParalelas}");
            //var ubi = @"C:\Users\ezequ\Desktop\ConsoleAppWithDI-master\ConsoleAppWithDI.UI\Stock.CSV";
            _acmeCorporationContext.Database.SetCommandTimeout(1000);
            _acmeCorporationContext.Database.ExecuteSqlRaw("BULK INSERT dbo.Stock FROM '" + resultado.UbicacionArchivo + "'" +
                "WITH  (MAXERRORS=10, FIRSTROW=2, FIELDTERMINATOR = ';',ROWTERMINATOR = '0x0a')");

            _logger.LogInformation("Descarga Finalizada");

        }
    }
}