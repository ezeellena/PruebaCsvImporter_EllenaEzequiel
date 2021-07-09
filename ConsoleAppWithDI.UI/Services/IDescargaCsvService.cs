using System;
using CsvImporter.Entities;

namespace CsvImporter.Services
{
    public interface IDescargaCsvService
    {
        ResultadoDescarga DescargarCsv(string Url, string CarpetaDestino, int NumeroDescargaParalelas = 0, bool ValidarSSL = false);
        public abstract ResultadoDescarga Descarga();
    }
}