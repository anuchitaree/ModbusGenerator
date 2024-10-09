using EasyModbus;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace ModbusGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ModbusClient modbusClient = new ModbusClient();
        ModbusServer modbusServer;

        DispatcherTimer timerPollWrite = new DispatcherTimer();
        DispatcherTimer timerPollRead = new DispatcherTimer();

        int powerEnergy = 0;
        int prodCapture = 0;

        public ChartValues<double> Values1 { get; set; }
        public ChartValues<double> Values2 { get; set; }


        public MainWindow()
        {
            InitializeComponent();

            chart1.Values = new ChartValues<int>();
            chart2.Values = new ChartValues<int>();



        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            modbusServer = new ModbusServer();
            modbusServer.Listen();

            Thread.Sleep(1000);

            modbusClient.IPAddress = "127.0.0.1";
            modbusClient.Port = 502;
            modbusClient.Connect();

            timerPollRead.Interval = TimeSpan.FromMilliseconds(5000);
            timerPollRead.Tick += TimerPollRead_Tick;

            timerPollWrite.Interval = TimeSpan.FromMilliseconds(5000);
            timerPollWrite.Tick += TimerPollWrite_Tick;

            timerPollWrite.Start();
            timerPollRead.Start();
        }

        private void TimerPollRead_Tick(object sender, EventArgs e)
        {
            try
            {
                if (modbusClient.Connected == true)
                {
                    int[] vals = modbusClient.ReadHoldingRegisters(0, 4);
                    //Console.WriteLine($"vals[0]: {vals[0]} ,vals[1]:{vals[1]}");

                    int powerEnergy = vals[0] << 16 | vals[1];
                    int prodCapture = vals[2] << 16 | vals[3];
                    //gauge1.Value = powerEnergy;
                    lbPowerEnergy.Content = $"Power energy (kw-h) : {powerEnergy.ToString()} [int32]";
                    lbProdCapture.Content = $"Production capture  : {prodCapture.ToString()} [int32]";


                    chart1.Values.Add(powerEnergy);
                    chart2.Values.Add(prodCapture);


                   var len = chart1.Values.Count;
                    if(chart1.Values.Count > 10)
                    {
                        chart1.Values.RemoveAt(0);
                        chart2.Values.RemoveAt(0);
                    }


                   


                }

            }
            catch (Exception ex)
            {

                ex.ToString();
            }
        }

     

        private void TimerPollWrite_Tick(object sender, EventArgs e)
        {
            // 400 000
            //short ival = short.Parse("15");
            //ModbusServer.HoldingRegisters regs = modbusServer.holdingRegisters;
            //regs[2] = ival;

            Random rnd = new Random();
            int energy = rnd.Next(1, 3);
            powerEnergy = powerEnergy + energy;
            byte[] bytes = BitConverter.GetBytes(powerEnergy);
            short firstHalf = BitConverter.ToInt16(bytes, 0);
            short secondHalf = BitConverter.ToInt16(bytes, 2);

            prodCapture += 1;
            byte[] bytes1 = BitConverter.GetBytes(prodCapture);
            short firstHalf1 = BitConverter.ToInt16(bytes1, 0);
            short secondHalf1 = BitConverter.ToInt16(bytes1, 2);


            short ival = short.Parse("15");
            ModbusServer.HoldingRegisters regs = modbusServer.holdingRegisters;
            regs[2] = firstHalf;
            regs[1] = secondHalf;

            regs[4] = firstHalf1;
            regs[3] = secondHalf1;

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            timerPollWrite.Stop();
            timerPollRead.Stop();

        }
    }
}
