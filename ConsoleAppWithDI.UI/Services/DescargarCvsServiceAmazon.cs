using ConsoleAppWithDI.UI.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleAppWithDI.UI.Services
{
    public class DescargarCvsServiceAmazon : IDescargaCsvService
    {
        public ResultadoDescarga Descarga()
        {
            throw new NotImplementedException();
        }

        public ResultadoDescarga DescargarCsv(string Url, string CarpetaDestino, int NumeroDescargaParalelas = 0, bool ValidarSSL = false)
        {
            throw new NotImplementedException();
        }
    }
}
