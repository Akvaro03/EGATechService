using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EGATechService
{
    public class ApiClass
    {
        ConfigJson configJson;
        const string ApiURL = "https://api.thingspeak.com";


        public ApiClass (ConfigJson configJson)
        {
            this.configJson = configJson;
        }

        /// <summary>
        /// Establece la ruta de la api en base al apikey del config.json
        /// y depende del canal que queramos solicitar de la API de Thingspeak
        /// </summary>
        /// <param name="channelID">Canal de la API a obtener los datos</param>
        /// <param name="configJson">Clase de configuracion del Json</param>
        /// <returns>Url de la solicitud completa</returns>
        private string setApiURL(int channelID, int apiKey)
        {
            string channelFieldRoute = $"/channels/{apiKey}/fields/{channelID}.json?results=1";
            return $"{ApiURL}{channelFieldRoute}";
        }

        /// <summary>
        /// Obtiene la temperatura medida desde la API de Thingspeak
        /// </summary>
        /// <param name="channelID">Canal de la API a obtener los datos de tipo </param>
        /// <returns>La ultima medicion brindada por la API</returns>
        public async Task<Feed> getChannelField(int channelID, int apiKey)
        {

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{setApiURL(channelID, apiKey)}");
                var content = await response.Content.ReadAsStringAsync();

                Root res = JsonSerializer.Deserialize<Root>(content);

                return res.feeds[0];

            }
        }

    }
}
