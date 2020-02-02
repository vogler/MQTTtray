using System;
using System.Drawing;
using System.Windows.Forms;

namespace MQTTtray
{
    static class Program
    {
        private static NotifyIcon ni;

        public static void Main(string[] astrArg)
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
            ni.Icon = new Icon(SystemIcons.Application, 40, 40);
            setIconText(1300);
            ni.Text = "MQTTtray settings";
            ni.Visible = true;
            ni.ContextMenu = cm;
            ni.DoubleClick += new EventHandler(Settings);

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
    }
}
