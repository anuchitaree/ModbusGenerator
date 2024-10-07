using EasyModbus;
using LiveCharts.Wpf;
using System;
using System.Net;
using System.Threading;
using System.Windows;
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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            modbusServer = new ModbusServer();
            modbusServer.Listen();
            
            Thread.Sleep(1000);

            modbusClient.IPAddress = "127.0.0.1";
            modbusClient.Port = 502;
            modbusClient.Connect();

            timerPollRead.Interval = TimeSpan.FromSeconds(1);
            timerPollRead.Tick += TimerPollRead_Tick;

            timerPollWrite.Interval = TimeSpan.FromSeconds(1);
            timerPollWrite.Tick += TimerPollWrite_Tick;

            timerPollRead.Start();
            timerPollWrite.Start();
        }

        private void TimerPollRead_Tick(object sender, EventArgs e)
        {
            if (modbusClient.Connected == true)
            {
                int[] vals = modbusClient.ReadHoldingRegisters(0, 4);
                Console.WriteLine($"vals[0]: {vals[0]} ,vals[1]:{vals[1]}");

                int powerEnergy = vals[1] << 16 | vals[0];
                int prodCapture = vals[3] << 16 | vals[2];


                gauge1.Value = powerEnergy;

            }
        }


        private void TimerPollWrite_Tick(object sender, EventArgs e)
        {
            // 400 000
            //short ival = short.Parse("15");
            //ModbusServer.HoldingRegisters regs = modbusServer.holdingRegisters;
            //regs[2] = ival;

            Random rnd = new Random();
            int energy = rnd.Next(1,3);
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
            regs[1] = firstHalf;
            regs[2] = secondHalf;

            regs[3] = firstHalf1;
            regs[4] = secondHalf1;

        }



    }
}
