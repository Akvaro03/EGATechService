using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGATechService
{
    /// <summary>
    /// Mediciones del API de Thingspeak
    /// </summary>
    public class Feed
    {
        public DateTime created_at { get; set; }
        public int entry_id { get; set; }
        public string field1 { get; set; }
        public string field2 { get; set; }
    }
}
