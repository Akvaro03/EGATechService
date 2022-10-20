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
using System.Threading.Tasks;
using System.Timers;
namespace EGATechService
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();

        const string ApiURL = "https://api.thingspeak.com";
        const string ApiKey = "1889060";

        public Service1()
        {
            InitializeComponent();
        }

        private string setApiURL(int channelID)
        {
            string channelFieldRoute = $"/channels/{ApiKey}/fields/{channelID}.json?results=1";
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
            WriteToFile("Inicio del servicio" + DateTime.Now);
            WriteToFile(" ; Temperatura ; Humedad; Fecha");

            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000; //number in milisecinds  
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


        private async void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            Feed feed = await getChannelField(1);
            double temperature = ParseTemperature(feed.field1);

            WriteToFile(" ; " + temperature.ToString() + "; 20% ; " + feed.created_at, temperature);
        }


        public void WriteToFile(string Message, double temp = 0)
        {
            string path = @"C:\EGATechLogs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = path + "\\ServiceLog_" + temp + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".csv";
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
