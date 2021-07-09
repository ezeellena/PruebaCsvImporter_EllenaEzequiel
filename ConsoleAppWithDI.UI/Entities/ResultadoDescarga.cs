using System;

namespace CsvImporter.Entities
{
    public class ResultadoDescarga
    {
        public long Tama�o { get; set; }
        public String UbicacionArchivo { get; set; }
        public TimeSpan TiempoDemora { get; set; }
        public int DescargasParalelas { get; set; }
    }
}