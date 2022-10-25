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
        Timer timer = new Timer();

        const string ApiURL = "https://api.thingspeak.com";

        ConfigJson configJson;
        public Service1()
        {
            InitializeComponent();
            configJson = new ConfigJson();
        }

        private string setApiURL(int channelID)
        {
            string channelFieldRoute = $"/channels/{configJson.ApiKey}/fields/{channelID}.json?results=1";
            return $"{ApiURL}{channelFieldRoute}";
        }

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
            verifyIfConfigJsonExist();
            WriteToFile("Inicio del servicio" + DateTime.Now);
            WriteToFile(" ; Temperatura ; Humedad; Fecha");

            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 40000; //number in milisecinds  
            timer.Enabled = true;
        }
        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }
        public double ParseTemperature (string temp)
        {
            string temperatureParsed = temp.Substring(0, 5);
            return Math.Round(float.Parse(temperatureParsed, CultureInfo.InvariantCulture.NumberFormat), 2);
        }

        private DateTime ParseTimezone(DateTime tempDate)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(tempDate, TimeZoneInfo.Local);
        }

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
                using (StreamReader r = new StreamReader("file.json"))
                {
                    string json = r.ReadToEnd();
                    configJson = JsonSerializer.Deserialize<ConfigJson>(json);
                }
            }
        }


        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            verifyIfConfigJsonExist();

            if (configJson.ApiKey == null)
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


        private void verifyDirectory (string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void WriteToFile(string Message)
        {
            string logsPath = configJson.LogsPath;
            verifyDirectory(logsPath);

            string yearLogsPath = $"{logsPath}\\EGATech_{DateTime.Now.ToString("yyyy")}";
            verifyDirectory(yearLogsPath);

            string monthLogsPath = $"{yearLogsPath}\\EGATech_{DateTime.Now.ToString("MMMM")}";
            verifyDirectory(monthLogsPath);

            string filepath = monthLogsPath + "\\ServiceLog_" + DateTime.Now.ToShortDateString().Replace('/', '_') + ".csv";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                
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

    public class ConfigJson
    {
        public string ApiKey { get; set; }
        public string LogsPath { get; set; } = @"C:\EGATechLogs";

        //miliseconds
        public int IntervalTime { get; set; } = 40000;
    }

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

    public class Feed
    {
        public DateTime created_at { get; set; }
        public int entry_id { get; set; }
        public string field1 { get; set; }
    }

    public class Root
    {
        public Channel channel { get; set; }
        public IList<Feed> feeds { get; set; }
    }
}
