# Csv Importer
_Extracción de un archivo .csv almacenado en una cuenta de almacenamiento de Azure e insertar su contenido en una BD SQL Server local
## Comenzamos
### Instalar Los paquetes Necesarios

Instalacion.
<pre>
    <code>
       Microsoft.Extensions.DependencyInjection
       Microsoft.Extensions.Logging.Console
       Microsoft.Extensions.Configuration.Json
       Microsoft.EntityFrameworkCore
       Microsoft.EntityFrameworkCore.Tools
       Microsoft.EntityFrameworkCore.SqlServer
    </code>
</pre> 

En el Managment Studio Sql server Crear base de datos y crear la tabla Stock.
<pre>
    <code>
CREATE DATABASE AcmeCorporation
    USE [AcmeCorporation]
GO

/****** Object:  Table [dbo].[Stock]    Script Date: 04/07/2021 18:11:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Stock](
	[PointOfSale] [varchar](50) NULL,
	[Product] [varchar](50) NULL,
	[Date] [datetime] NULL,
	[Stock] [smallint] NULL
) ON [PRIMARY]
GO
    </code>
</pre>
Una vez creada la tabla y la base nos vamos a la aplicacion de consola y creamos el contexto.
<pre>
    <code>
Scaffold-DbContext "Server="{NombreServidor}";Database=AcmeCorporation;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
    </code>
</pre>

Como en un ASP. NET Core, configuraremos varios servicios y crearemos una instancia de un proveedor. 
Así que comencemos creando la clase Startup en un archivo Startup.cs en el mismo 
directorio que es nuestro Program.cs.
Contiene el servicio de la conexion a la base, el Serilog y los singletos para la inyeccion de dependecia
<pre>
    <code>
    using Microsoft.Extensions.DependencyInjection;

    namespace CsvImporter
    {
        public static class Startup
        {
            public static IServiceCollection ConfigureServices()
            {
                var services = new ServiceCollection();

                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

                IConfiguration configuration = builder.Build();
                services.AddSingleton(configuration);

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .WriteTo.File("Logging-.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                    .CreateLogger();

                services.AddLogging(builder =>
                {
                    builder.AddConfiguration(configuration.GetSection("Logging"));
                    builder.AddConsole();
                    builder.AddSerilog();
                });

                services.AddDbContext<AcmeCorporationContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("AcmeCorporation"));
                });

                services.AddSingleton<IDescargaCsvService, DescargaCsvService>();

                services.AddSingleton<Csv>();

                return services;
            }
        }   
    }
    </code>
</pre>

Ahora agreguemos la clase para el punto de entrada de nuestra aplicación. 
Consulta si la tabla esta vacia o llena y contiene la funcion de descarga e insert a la BD.
<pre>
    <code>
        using System;
        using System.Linq;
        using ConsoleAppWithDI.UI.Models;
        using ConsoleAppWithDI.UI.Services;
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
                    var ubi = @"C:\Users\ezequ\Desktop\ConsoleAppWithDI-master\ConsoleAppWithDI.UI\Stock.CSV";
                    _acmeCorporationContext.Database.SetCommandTimeout(1000);
                    _acmeCorporationContext.Database.ExecuteSqlRaw("BULK INSERT dbo.Stock FROM '" + ubi + "'" +
                        "WITH  (MAXERRORS=10, FIRSTROW=2, FIELDTERMINATOR = ';',ROWTERMINATOR = '0x0a')");

                    _logger.LogInformation("Descarga Finalizada");

                }
            }
        }
    </code>
</pre>

La funcion DescargaEInsert obtiene la ruta del csv del json,
La ruta donde va a guardar el csv descargado y 
La funcion de descarga y el insert a la base
<pre>
    <code>
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
        var ubi = @"C:\Users\ezequ\Desktop\ConsoleAppWithDI-master\ConsoleAppWithDI.UI\Stock.CSV";
        _acmeCorporationContext.Database.SetCommandTimeout(1000);
        _acmeCorporationContext.Database.ExecuteSqlRaw("BULK INSERT dbo.Stock FROM '" + ubi + "'" +
            "WITH  (MAXERRORS=10, FIRSTROW=2, FIELDTERMINATOR = ';',ROWTERMINATOR = '0x0a')");

        _logger.LogInformation("Descarga Finalizada");

    }
    </code>
</pre>

Para que la logica anterior funcione debemos implementar la inyeccion de dependencia.
Para eso hay que crear la clase ResultadoDescarga
<pre>
    <code>
    namespace CsvImporter
    {
        public class ResultadoDescarga
        {
            public long Tamaño { get; set; }
            public String UbicacionArchivo { get; set; }
            public TimeSpan TiempoDemora { get; set; }
            public int DescargasParalelas { get; set; }
        }
    }
    </code>
</pre>
Ahora agregaremos la Interfaz para nuestro servicio.
<pre>
    <code>
    namespace ConsoleAppWithDI.UI.Services
        {
            public interface IDescargaCsvService
            {
                ResultadoDescarga DescargarCsv(string Url, string CarpetaDestino, int NumeroDescargaParalelas = 0, bool ValidarSSL = false);
            }
        }
    </code>
</pre>

Y el servicio actual.
Esto fue un intento de implementar una descarga en paralelo, aparentemente funciona 
pero crea saltos de linea que al insertar en la base de datos genera unos errores.
<pre>
    <code>
    public class DescargaCsvService : IDescargaCsvService
    {
        static DescargaCsvService()
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.MaxServicePointIdleTime = 1000;

        }
        internal class Range
        {
            public long Start { get; set; }
            public long End { get; set; }
        }
        public ResultadoDescarga DescargarCsv(string Url, string CarpetaDestino, int NumeroDescargaParalelas = 0, bool ValidarSSL = false)
        {
            if (!ValidarSSL)
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }

            Uri uri = new Uri(Url);

            //Calculate destination path  
            String destinationFilePath = Path.Combine(CarpetaDestino, uri.Segments.Last());

            ResultadoDescarga resultado = new ResultadoDescarga() {  UbicacionArchivo = destinationFilePath };

            //Handle number of parallel downloads  
            if (NumeroDescargaParalelas <= 0)
            {
                NumeroDescargaParalelas = Environment.ProcessorCount;
            }

            #region Obtener el tamaño del archivo
            WebRequest webRequest = HttpWebRequest.Create(Url);
            webRequest.Method = "HEAD";
            var milisegundos = new TimeSpan(0, 15, 0).TotalMilliseconds;
            webRequest.Timeout = (int)milisegundos;
            long responseLength;
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                responseLength = long.Parse(webResponse.Headers.Get("Content-Length"));
                resultado.Tamaño = responseLength;
            }
            #endregion

            if (File.Exists(destinationFilePath))
            {
                File.Delete(destinationFilePath);
            }

            using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Append))
            {
                ConcurrentDictionary<int, String> tempFilesDictionary = new ConcurrentDictionary<int, String>();

                #region Calculate ranges  
                List<Range> readRanges = new List<Range>();
                for (int chunk = 0; chunk < NumeroDescargaParalelas - 1; chunk++)
                {
                    var range = new Range()
                    {
                        Start = chunk * (responseLength / NumeroDescargaParalelas),
                        End = ((chunk + 1) * (responseLength / NumeroDescargaParalelas)) - 1
                    };
                    readRanges.Add(range);
                }


                readRanges.Add(new Range()
                {
                    Start = readRanges.Any() ? readRanges.Last().End + 1 : 0,
                    End = responseLength - 1
                });

                #endregion

                DateTime startTime = DateTime.Now;

                #region Parallel download  

                int index = 0;
                Parallel.ForEach(readRanges, new ParallelOptions() { MaxDegreeOfParallelism = NumeroDescargaParalelas }, readRange =>
                {
                    HttpWebRequest httpWebRequest = HttpWebRequest.Create(Url) as HttpWebRequest;
                    httpWebRequest.Method = "GET";
                    httpWebRequest.AddRange(readRange.Start, readRange.End);
                    using (HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse)
                    {
                        String tempFilePath = Path.GetTempFileName();
                        using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                        {
                            httpWebResponse.GetResponseStream().CopyTo(fileStream);
                            tempFilesDictionary.TryAdd((int)index, tempFilePath);
                        }
                    }
                    index++;

                });

                resultado.DescargasParalelas = index;

                #endregion

                resultado.TiempoDemora = DateTime.Now.Subtract(startTime);

                #region Merge to single file  
                foreach (var tempFile in tempFilesDictionary.OrderBy(b => b.Key))
                {
                    byte[] tempFileBytes = File.ReadAllBytes(tempFile.Value);
                    destinationStream.Write(tempFileBytes, 0, tempFileBytes.Length);
                    File.Delete(tempFile.Value);
                }
                #endregion


                return resultado;
            }


        }

        
    }
    </code>
</pre>



## Autor ✒️


* **Ellena Ezequiel** - *Trabajo Inicial* - ezeellena(https://github.com/ezeellena)
* **Ellena Ezequiel** - *Documentación* - ezeellena(https://github.com/ezeellena)


---
⌨️ con ❤️ por [Ezequiel Ellena](https://github.com/ezeellena) 😊