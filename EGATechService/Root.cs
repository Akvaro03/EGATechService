using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EGATechService
{
    /// <summary>
    /// Tipo de objeto que devuelve el API de Thingspeak
    /// </summary>
    public class Root
    {
        public Channel channel { get; set; }
        public IList<Feed> feeds { get; set; }
    }
}
