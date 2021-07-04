using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ConsoleAppWithDI.UI.Entities;

namespace ConsoleAppWithDI.UI.Services
{
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
}