using System;
using System.Collections.Generic;

namespace CsvImporter.Models
{
    public partial class Stock
    {
        public string PointOfSale { get; set; }
        public string Product { get; set; }
        public DateTime? Date { get; set; }
        public short? Stock1 { get; set; }
    }
}
