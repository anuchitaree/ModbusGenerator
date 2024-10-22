using EasyModbus;
using LiveCharts;
using System;
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

        UInt32 powerEnergy = 0;
        UInt32 prodCapture = 0;

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

            timerPollRead.Interval = TimeSpan.FromMilliseconds(1000);
            timerPollRead.Tick += TimerPollRead_Tick;

            timerPollWrite.Interval = TimeSpan.FromMilliseconds(1000);
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

                    UInt32 powerEnergy0 = (UInt32)vals[0] << 16;
                    UInt32 powerEnergy1 = (UInt32)vals[1];
                    UInt32 powerEnergy = powerEnergy0 | (powerEnergy1 & 0x0000FFFF); //2147483647



                    UInt32 prodCapture0 = (UInt32)vals[2] << 16;
                    UInt32 prodCapture1 = (UInt32)vals[3];
                    UInt32 prodCapture = prodCapture0 | (prodCapture1 & 0x0000FFFF);


                    //string hexValue1 = vals[0].ToString("X");  // 0000 7FFF
                    //string hexValue2 = vals[1].ToString("X"); // FFFF FFFF


                    //gauge1.Value = powerEnergy;
                    lbPowerEnergy.Content = $"Power energy (kw-h) : {powerEnergy:n0} [UInt32]";
                    lbProdCapture.Content = $"Production capture  : {prodCapture:n0} [UInt32]";


                    chart1.Values.Add((int)powerEnergy);
                    chart2.Values.Add((int)prodCapture);


                    var len = chart1.Values.Count;
                    if (chart1.Values.Count > 10)
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

            if (powerEnergy > 999999999)
            {
                powerEnergy = 0;
            }

            if (prodCapture > 999999999)
            {
                prodCapture = 0;
            }

            Random rnd = new Random();
            int energy = rnd.Next(1, 3);
            UInt32 randomEnergy = Convert.ToUInt32(energy);

            powerEnergy = powerEnergy + randomEnergy;

            //powerEnergy = 2147483647;  // 7FFF FFFF

            byte[] bytes = BitConverter.GetBytes(powerEnergy);
            short firstHalf = BitConverter.ToInt16(bytes, 0);
            short secondHalf = BitConverter.ToInt16(bytes, 2);


            //string hexValue1 = firstHalf.ToString("X");
            //string hexValue2 = secondHalf.ToString("X");



            prodCapture += 1;
            byte[] bytes1 = BitConverter.GetBytes(prodCapture);
            short firstHalf1 = BitConverter.ToInt16(bytes1, 0);
            short secondHalf1 = BitConverter.ToInt16(bytes1, 2);


            //short ival = short.Parse("15");
            ModbusServer.HoldingRegisters regs = modbusServer.holdingRegisters;
            regs[2] = firstHalf;  /// FFFF
            regs[1] = secondHalf;  /// 7FFF

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
