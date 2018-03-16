using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace F1Dashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    delegate void ConvertMethod(byte[] packet);
    public partial class MainWindow : Window
    {
        private UDPServer server;
        private Thread listenThread;
        private ConvertMethod OnPacketReceivedMethod;
        private int speed_max = 0;
        private F1Data data;
        private F1Data prevData;
  
        public MainWindow()
        {
            OnPacketReceivedMethod = OnPacketReceived;
            server = new UDPServer(OnPacketReceivedMethod);
            listenThread = new Thread(() => server.listen());
            listenThread.Start();
            InitializeComponent();
        }

        private void OnPacketReceived(byte[] packet)
        {
            prevData = data;
            data = new F1Data(packet);
            // Calc
            float engineRate = (int)data.Get("engineRate");
            float engineRatePercent = engineRate * 100 / 14000;

            float speed = (int)(data.Get("speed") * 3.6);
            float speedPercent = speed * 100 / 400;

            float throttle = data.Get("throttle");
            float throttlePercent = throttle * 100 / 1;
            float brake = data.Get("brake");
            float brakePercent = brake * 100 / 1;

            // Update
            if (speed > speed_max) this.speed_max = (int)speed;

            // Draw
            this.Dispatcher.Invoke(() =>
            { 
                this.lbl_rpm.Content = String.Format("{0} RPM", engineRate, engineRatePercent);
                this.lbl_speed.Content = String.Format("{0} KM/h", speed);
                this.lbl_speed_max.Content = String.Format("max: {0} KM / h", speed_max); 
                this.lbl_time.Content = Formatter.Seconds(data.Get("time"));
                this.lbl_lapTime.Content = Formatter.Seconds(data.Get("lapTime"));
                this.lbl_sector1_time.Content = Formatter.Seconds(data.Get("sector1_time"));
                this.lbl_sector2_time.Content = Formatter.Seconds(data.Get("sector2_time"));
                this.lbl_last_lap_time.Content = Formatter.Seconds(data.Get("last_lap_time"));
                this.pbar_rpm.Value = engineRatePercent;
                this.pbar_speed.Value = speedPercent;
                this.pbar_throttle.Value = throttlePercent;
                this.pbar_brake.Value = brakePercent;
                this.lbl_gear.Content = Constants.GEARS[(int)data.Get("gear")];

                this.lbl_wheel_speed_bl.Content = data.Get("wheel_speed_bl");
                this.lbl_wheel_speed_br.Content = data.Get("wheel_speed_br");
                this.lbl_wheel_speed_fl.Content = data.Get("wheel_speed_fl");
                this.lbl_wheel_speed_fr.Content = data.Get("wheel_speed_fr");
                // this.dgrid_data.ItemsSource = LoadTableData(data);
            });
            
        }

        private List<Variable> LoadTableData(F1Data data)
        {
            List<Variable> vars = new List<Variable>();
            
            foreach(KeyValuePair<string, int> item in F1Data.VALUES)
            {
                vars.Add(new Variable() {
                    key = item.Key,
                    value = data.Get(item.Key)
                });
            }

            return vars;
        }
    }

    public class Variable
    {
        public string key { get; set; }
        public float value { get; set; }
    }
}
