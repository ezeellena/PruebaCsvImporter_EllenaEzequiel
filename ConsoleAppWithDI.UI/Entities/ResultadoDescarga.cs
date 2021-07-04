using System;

namespace ConsoleAppWithDI.UI.Entities
{
    public class ResultadoDescarga
    {
        public long Tamaño { get; set; }
        public String UbicacionArchivo { get; set; }
        public TimeSpan TiempoDemora { get; set; }
        public int DescargasParalelas { get; set; }
    }
}