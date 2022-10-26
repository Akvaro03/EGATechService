using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Timers;
namespace EGATechService
{
    public partial class Service1 : ServiceBase
    {
        /// <summary>
        /// Timer para leer los valores de la api y luego escribirlos en un archivo .csv
        /// </summary>
        Timer timerTemperatures = new Timer();

        /// <summary>Timer para verificar que exista el archivo json de configuracion y 
        /// los directorios correspondientes
        /// </summary>
        Timer verificationTimer = new Timer();

        const string ApiURL = "https://api.thingspeak.com";
        ConfigJson configJson;

        public Service1()
        {
            InitializeComponent();
            configJson = new ConfigJson();
        }

        /// <summary>
        /// Establece la ruta de la api en base al apikey del config.json
        /// y depende del canal que queramos solicitar de la API de Thingspeak
        /// </summary>
        /// <param name="channelID">Canal de la API a obtener los datos</param>
        /// <returns>Url de la solicitud completa</returns>
        private string setApiURL(int channelID)
        {
            string channelFieldRoute = $"/channels/{configJson.ApiKey}/fields/{channelID}.json?results=1";
            return $"{ApiURL}{channelFieldRoute}";
        }

        /// <summary>
        /// Obtiene la temperatura medida desde la API de Thingspeak
        /// </summary>
        /// <param name="channelID">Canal de la API a obtener los datos</param>
        /// <returns>La ultima medicion brindada por la API</returns>
        public async Task<Feed> getChannelField(int channelID)
        {

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{setApiURL(channelID)}");
                var content = await response.Content.ReadAsStringAsync();

                Root res = JsonSerializer.Deserialize<Root>(content);

                return res.feeds[0];
            }
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Inicio del servicio" + DateTime.Now);
            WriteToFile(" ; Temperatura ; Humedad; Fecha");

            timerTemperatures.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timerTemperatures.Interval = configJson.IntervalTime; //number in milisecinds  
            timerTemperatures.Enabled = true;

            verificationTimer.Elapsed += new ElapsedEventHandler(VerificationTimerElapsed);
            verificationTimer.Interval = 5000;
            verificationTimer.Enabled = true;
        }
        protected override void OnStop()
        {
            WriteToFile("El servicio se detuvo ;" + DateTime.Now);
        }

        /// <summary>
        /// Convierte la temperatura de tipo string a double
        /// </summary>
        /// <param name="temp">La temperatura a transformar</param>
        /// <returns>La temperatura parseada a double</returns>
        public double ParseTemperature (string temp)
        {
            string temperatureParsed = temp.Substring(0, 5);
            return Math.Round(float.Parse(temperatureParsed, CultureInfo.InvariantCulture.NumberFormat), 2);
        }

        /// <summary>
        /// Transforma la fecha y hora del Feed a la zona horaria local
        /// </summary>
        /// <param name="tempDate">La fecha del Feed</param>
        /// <returns>La fecha con nuestra zona horaria local</returns>
        private DateTime ParseTimezone(DateTime tempDate)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(tempDate, TimeZoneInfo.Local);
        }


        private void VerificationTimerElapsed (object source, ElapsedEventArgs e)
        {
            verifyIfConfigJsonExist();
            CheckIfPathsExist();
        }

        /// <summary>
        /// Verifica si el archivo Config.json existe, si este no existe lo crea y le escribe los valores de la clase 
        /// instanciada configJson. Si ya existe simplemente lee el archivo y lo escribe en la clase instanciada de configJson
        /// </summary>
        private void verifyIfConfigJsonExist()
        {
            string jsonPath = $"{configJson.LogsPath}\\Config.json";
            if (!File.Exists(jsonPath))
            {
                using (StreamWriter sw = File.CreateText(jsonPath))
                {
                    var jsonString = JsonSerializer.Serialize(configJson);
                    sw.Write(jsonString);

                }
            }
            else
            {
                string text = File.ReadAllText(jsonPath);
                var jsonconfig = JsonSerializer.Deserialize<ConfigJson>(text);
                configJson.LogsPath = jsonconfig.LogsPath;
                configJson.ApiKey = jsonconfig.ApiKey;
                configJson.IntervalTime = jsonconfig.IntervalTime;
            }
        }


        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            verifyIfConfigJsonExist();

            if (configJson.ApiKey == 0)
            {
                WriteToFile("Error ; APIKey nulo");
            } else
            {
                Task<Feed>feedTask = getChannelField(1);
                feedTask.Wait();
                Feed feed = feedTask.Result;

                double temperature = ParseTemperature(feed.field1);

                WriteToFile(" ; " + temperature.ToString() + "; 20% ; " + ParseTimezone(feed.created_at));
            }
        }

        /// <summary>
        /// Verifica si un directorio existe, si este no existe lo crea en el path indicado en el parametro
        /// </summary>
        /// <param name="path">ruta del directorio</param>
        private void verifyDirectory (string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Verifica ruta por ruta si existen los directorios o archivos, si no existen los crea y asi con todos.
        /// Las carpetas se crean por año y mes. El archivo .csv se crea por día.
        /// </summary>
        /// <returns>La ruta final en donde se va a encontrar el archivo .csv</returns>
        private string CheckIfPathsExist ()
        {
            string logsPath = configJson.LogsPath;
            verifyDirectory(logsPath);

            string yearLogsPath = $"{logsPath}\\EGATech_{DateTime.Now.ToString("yyyy")}";
            verifyDirectory(yearLogsPath);

            string monthLogsPath = $"{yearLogsPath}\\EGATech_{DateTime.Now.ToString("MMMM")}";
            verifyDirectory(monthLogsPath);

            string filepath = monthLogsPath + "\\ServiceLog_" + DateTime.Now.ToShortDateString().Replace('/', '_') + ".csv";
            return filepath;
        }

        /// <summary>
        /// Crea los directorios hasta el archivo .csv y escribe un mensaje dentro del csv
        /// </summary>
        /// <param name="Message">El mensaje a escribir dentro del csv.</param>
        public void WriteToFile(string Message)
        {
            string filepath = CheckIfPathsExist();
            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }

    /// <summary>
    /// Clase que indica los datos que va a contener el archivo json de configuración
    /// </summary>
    public class ConfigJson
    {
        public int ApiKey { get; set; } = 0;
        public string LogsPath { get; set; } = @"C:\EGATechLogs";

        //miliseconds
        public int IntervalTime { get; set; } = 40000;
    }

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

    /// <summary>
    /// Mediciones del API de Thingspeak
    /// </summary>
    public class Feed
    {
        public DateTime created_at { get; set; }
        public int entry_id { get; set; }
        public string field1 { get; set; }
    }

    /// <summary>
    /// Tipo de objeto que devuelve el API de Thingspeak
    /// </summary>
    public class Root
    {
        public Channel channel { get; set; }
        public IList<Feed> feeds { get; set; }
    }
}
