using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGATechService
{
    /// <summary>
    /// Datos del canal configurado desde la 
    /// </summary>
    public class Channel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string field1 { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int last_entry_id { get; set; }
    }
}
