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
    public partial class ServicioMain : ServiceBase
    {
        /// <summary>
        /// Timer para leer los valores de la api y luego escribirlos en un archivo .csv
        /// </summary>
        Timer timerTemperatures = new Timer();

        /// <summary>Timer para verificar que exista el archivo json de configuracion y 
        /// los directorios correspondientes
        /// </summary>
        Timer verificationTimer = new Timer();

        Verifications verifications = new Verifications();
        ApiClass apiClass;

        ConfigJson configJson;

        public ServicioMain()
        {
            InitializeComponent();
            configJson = new ConfigJson();
            apiClass = new ApiClass();
        }

        
        protected override void OnStart(string[] args)
        {
            foreach (int key in configJson.ApiKey)
            {
                string filepath = verifications.CheckIfPathsExist(configJson, key);
                WriteToFile("Inicio del servicio ; " + DateTime.Now, filepath);

            }

            timerTemperatures.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timerTemperatures.Interval = configJson.IntervalTime; //number in milisecinds  
            timerTemperatures.Enabled = true;

            verificationTimer.Elapsed += new ElapsedEventHandler(VerificationTimerElapsed);
            verificationTimer.Interval = 5000;
            verificationTimer.Enabled = true;
        }
        protected override void OnStop()
        {
            //WriteToFile("El servicio se detuvo ;" + DateTime.Now);
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
            verifications.verifyIfConfigJsonExist(configJson);
            foreach(int key in configJson.ApiKey) { 
                verifications.CheckIfPathsExist(configJson, key);
            }
        }

       
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            verifications.verifyIfConfigJsonExist(configJson);

           
            foreach (int key in configJson.ApiKey)
            {
                string filepath = verifications.CheckIfPathsExist(configJson, key);
                try {
                    Task<Feed> TaskTemperatures = apiClass.getChannelField(1, key);
                    TaskTemperatures.Wait();
                    Feed feedTemp = TaskTemperatures.Result;

                    Task<Feed> TaskHumidity = apiClass.getChannelField(2, key);
                    TaskHumidity.Wait();
                    Feed feedHumidity = TaskHumidity.Result;

                    string temperaturaFinal = feedTemp.field1.Trim();
                    string humidityFinal = feedHumidity.field2.Trim();
                    DateTime timeCreated = ParseTimezone(feedTemp.created_at);

                    if (feedTemp.field1 == null)
                    {
                        temperaturaFinal = "Error de la temperatura";
                    }
                    else
                    {
                        temperaturaFinal = temperaturaFinal.Replace("\r\n", string.Empty);
                    }


                    if (feedHumidity.field2 == null)
                    {
                        humidityFinal = "Error en Humedad";

                    }
                    string strToSend = $" ; {temperaturaFinal} ; {humidityFinal} ; {timeCreated}";
                    WriteToFile($"{strToSend}", filepath);
                }
                catch
                {
                    WriteToFile("API Error", filepath);
                }

                
            }
            
        }

        
        /// <summary>
        /// Crea los directorios hasta el archivo .csv y escribe un mensaje dentro del csv
        /// </summary>
        /// <param name="Message">El mensaje a escribir dentro del csv.</param>
        /// <param name="path">La ruta del archivo en donde se va a escribir el mensaje</param>
        public void WriteToFile(string Message, string path)
        {
            
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(Message);
                }
            }
            
            
        }
    }
}
