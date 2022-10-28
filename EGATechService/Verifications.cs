using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EGATechService
{
    public class Verifications
    {
        /// <summary>
        /// Verifica si un directorio existe, si este no existe lo crea en el path indicado en el parametro
        /// </summary>
        /// <param name="path">ruta del directorio</param>
        public void verifyDirectory(string path)
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
        /// <param name="configJson">Objeto de configuracion del json con el que se va obtener la ruta principal de todos los directorios</param>
        /// <param name="ApiKey">El key con la que se va a nombrar el directorio</param>
        /// <returns>La ruta final en donde se va a encontrar el archivo .csv</returns>
        public string CheckIfPathsExist(ConfigJson configJson, int ApiKey)
        {

            string logsPath = configJson.LogsPath;
            verifyDirectory(logsPath);

            string yearLogsPath = $"{logsPath}\\EGATech_{DateTime.Now.ToString("yyyy")}";
            verifyDirectory(yearLogsPath);

            string monthLogsPath = $"{yearLogsPath}\\EGATech_{DateTime.Now.ToString("MMMM")}";
            verifyDirectory(monthLogsPath);

            string apiKeyLogsPath = $"{monthLogsPath}\\API_{ApiKey}";
            verifyDirectory(apiKeyLogsPath);

            return apiKeyLogsPath + "\\ServiceLog_" + DateTime.Now.ToShortDateString().Replace('/', '_') + ".csv";

        }

        /// <summary>
        /// Verifica si el archivo Config.json existe, si este no existe lo crea y le escribe los valores de la clase 
        /// instanciada configJson. Si ya existe simplemente lee el archivo y lo escribe en la clase instanciada de configJson
        /// </summary>
        public void verifyIfConfigJsonExist(ConfigJson configJson)
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
    }
}
