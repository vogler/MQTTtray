// required for tray icon
using System;
using System.Drawing;
using System.Windows.Forms;

// required for MQTT subscribe and JSON parse
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace MQTTtray
{
    static class Program
    {
        // TODO set via UI in Settings.cs
        private static string server = "rpi3";
        private static string topic = "sensors/mh-z19b";
        private static string field = "co2";

        private static NotifyIcon ni;

        public static async Task Main(string[] astrArg)
        {
            var cm = new ContextMenu();

            // detailed version add (want it to be bold (DefaultItem))
            var mi = new MenuItem();
            mi.Text = "&Settings";
            mi.DefaultItem = true;
            mi.Click += new System.EventHandler(Settings);
            cm.MenuItems.Add(mi);

            // short version add
            cm.MenuItems.Add("&Exit", Exit);

            ni = new NotifyIcon();
            ni.Icon = new Icon(SystemIcons.Question, 40, 40);
            //setIconText(1234);
            ni.Text = "MQTTtray settings";
            ni.Visible = true;
            ni.ContextMenu = cm;
            ni.DoubleClick += new EventHandler(Settings);

            await ConnectAsync();
            Application.Run();
        }

        private static void setIconText(int n)
        {
            // https://stackoverflow.com/questions/36379547/writing-text-to-the-system-tray-instead-of-an-icon
            Font fontToUse = new Font("Trebuchet MS", 20, FontStyle.Bold, GraphicsUnit.Pixel);
            var color = n > 2000 ? Color.Red : n > 1200 ? Color.Orange : n > 800 ? Color.Yellow : Color.LightGreen;
            Brush brushToUse = new SolidBrush(color);
            Bitmap bitmapText = new Bitmap(40, 40);
            Graphics g = System.Drawing.Graphics.FromImage(bitmapText);
            g.Clear(Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString(n.ToString(), fontToUse, brushToUse, -5, 5);
            IntPtr hIcon = (bitmapText.GetHicon());
            ni.Icon = System.Drawing.Icon.FromHandle(hIcon);
            //DestroyIcon(hIcon.ToInt32);
        }

        private static void Exit(Object sender, EventArgs e)
        {
            ni.Dispose();
            Application.Exit();
        }

        private static void Settings(Object sender, EventArgs e)
        {
            new Settings().Show();
        }

        private static async Task ConnectAsync()
        {
            // Setup and starts a managed MQTT client.
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("Windows-MQTTtray")
                    .WithTcpServer(server)
                    //.WithTls()
                    .Build())
                .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());
            _ = mqttClient.UseApplicationMessageReceivedHandler(e =>
              {
                  //Debug.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                  //Debug.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                  //Debug.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                  //Debug.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                  //Debug.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                  var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                  var json = JObject.Parse(payload);
                  var value = json.Value<int>(field);
                  Debug.WriteLine($"{field}: {value}");
                  setIconText(value);
              });
            await mqttClient.StartAsync(options);
        }
    }
}
