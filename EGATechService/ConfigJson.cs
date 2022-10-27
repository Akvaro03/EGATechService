using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGATechService
{
    /// <summary>
    /// Clase que indica los datos que va a contener el archivo json de configuración
    /// </summary>
    public class ConfigJson
    {
        public List<int> ApiKey { get; set; } = new List<int>() { 1000 };
        public string LogsPath { get; set; } = @"C:\EGATechLogs";

        //miliseconds
        public int IntervalTime { get; set; } = 40000;
    }

}
