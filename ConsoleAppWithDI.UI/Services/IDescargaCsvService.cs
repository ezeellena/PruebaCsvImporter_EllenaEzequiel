using System;
using ConsoleAppWithDI.UI.Entities;

namespace ConsoleAppWithDI.UI.Services
{
    public interface IDescargaCsvService
    {
        ResultadoDescarga DescargarCsv(string Url, string CarpetaDestino, int NumeroDescargaParalelas = 0, bool ValidarSSL = false);
    }
}