using Dijing.Common.Core.Enums;
using Dijing.Common.Core.Utility;
using Dijing.CommunicationHelper;
using Dijing.SerialPortHelper;
using Dijing.SerilogExt;
using Serilog;
using System.Device.Gpio;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace test
{
    /// <summary>
    /// 程序入口
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 主函数
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Trace.Listeners.Clear(); // 清除所有 TraceListeners
            InitLog.SetLog(RunModeEnum.Debug);  //日志配置

            //TCP客户端测试
            //Task.Run(TCPClientTest);

            //串口测试
            Task.Run(RS485Test);

            //循环等待，禁止程序退出
            while(true)
            {
                Task.Delay(1000).Wait();
            }
        }



        //-------------TCP客户端示例------------------------------------

        private static void TCPClientTest()
        {
            var ip="127.0.0.1";
            var port = 7000;

            var handler = new DataHandler(1);
            handler.OnClientOfflineEvent += Handler_OnClientOfflineEvent;
            handler.OnClientOnlineEvent += Handler_OnClientOnlineEvent;
            handler.OnGetTCPDataEvent += Handler_OnGetTCPDataEvent;
            var channel = TCPClientHelper.Default.ConnectToServer(ip, port, handler);
            Task.Delay(1000).Wait();

            //TCP数据发送
            while(true)
            {
                var buff = Encoding.ASCII.GetBytes($"tcp client data,time now is {DateTime.Now}");
                TCPClientHelper.Default.SendDataAsync(buff);
                Task.Delay(2000).Wait();
            }
        }

        private static void Handler_OnClientOnlineEvent(object? sender, string e)
        {
            Serilog.Log.Information("客户端连接TCP服务器--上线完成");
        }

        private static void Handler_OnClientOfflineEvent(object? sender, Dijing.ComContext.Core.ContextView e)
        {
            Serilog.Log.Information("客户端连接TCP服务器--离线完成");
        }

        private static void Handler_OnGetTCPDataEvent(object? sender, Dijing.ComContext.Core.Context e)
        {
            //TCP数据接收

            //清理缓存数据，需要对接协议解析模块
            e.AssembleToRecvBuff();
            e.DataBuffList.Clear();
        }


        //-------------串口示例------------------------------------

        private static void RS485Test()
        {
            var name = "COM3"; //树莓派更换成 /dev/ttyS0类似这样的地址
            var baud = 9600;

            var sp = new SerialPortHelper("rasberry");
            sp.OnGetSerialPortDataEvent += Sp_OnGetSerialPortDataEvent;
            sp.Open(name,baud);
            Task.Delay(1000).Wait();

            //串口数据发送
            while (true)
            {
                var buff = Encoding.ASCII.GetBytes($"rs485 client data,time now is {DateTime.Now}");
                sp.SendData(buff);
                Task.Delay(2000).Wait();
            }

        }

        private static void Sp_OnGetSerialPortDataEvent(object? sender, Dijing.ComContext.Core.Context e)
        {
            //串口数据接收

            //清理缓存数据，需要对接协议解析模块
            e.AssembleToRecvBuff();
            e.DataBuffList.Clear();
        }

        //-------------gpio示例------------------------------------
        private static void GPIOTest()
        {
            int pin = 7;    //GPIO的引脚，查一下图            
            var controller = new GpioController();

            //输出模式
            controller.OpenPin(pin, PinMode.Output);    //输出模式
            for (int i = 0; i < 10; i++)
            {
                controller.Write(pin, PinValue.High);   //输出高电平
                Task.Delay(1000).Wait();

                controller.Write(pin, PinValue.Low);   //输出低电平
                Task.Delay(1000).Wait();
            }

            //输入模式
            //controller.OpenPin(pin, PinMode.Input);     //输入模式，未打开的情况下打开
            controller.SetPinMode(pin, PinMode.Input);  //输入模式，打开的情况下变更输入输出模式
            for (int i = 0; i < 10; i++)
            {
                PinValue value= controller.Read(pin);
                Serilog.Log.Information(value.ToString());  //public override string ToString() => _value == 0 ? "Low" : "High";
                Task.Delay(1000).Wait();
            }

            Serilog.Log.Information("gpio测试完成");
        }


        //树莓派安装dotnet

        //脚本连接
        //https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh

        //脚本运行,并指定安装目录
        //./dotnet-install.sh --install-dir /dotnet

        //设置环境变量
        //export DOTNET_ROOT =$HOME/.dotnet
        //export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
    }
}
