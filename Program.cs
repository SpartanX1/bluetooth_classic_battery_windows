using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using Windows.Networking.Sockets;
using System.Threading;

namespace BlueClassicWin10
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Available paired devices..");
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelectorFromPairingState(true));

            if (devices.Count == 0)
            {
                Console.WriteLine("No devices found");
                Exit();
            }

            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine(String.Format("{0}. {1}\t{2}", i, devices[i].Name, devices[i].Id));
            }

            Console.WriteLine("Select a device to connect..");
            int selectedDevice = Int32.Parse(Console.ReadLine());

            BluetoothDevice blDevice = await BluetoothDevice.FromIdAsync(devices[selectedDevice].Id);

            if (blDevice != null)
            {
                Console.WriteLine("Discovering Rfcomm services..");
                RfcommDeviceServicesResult rfcommResult = await blDevice.GetRfcommServicesAsync();
                if (rfcommResult.Services.Count == 0)
                {
                    Console.WriteLine("No services found");
                    Exit();
                }
                for (int i = 0; i < rfcommResult.Services.Count; i++)
                {
                    Console.WriteLine(String.Format("{0}. {1}", i, rfcommResult.Services[i].ServiceId.Uuid));
                }
                Console.WriteLine("Select a service to connect..");
                int selectedService = Int32.Parse(Console.ReadLine());

                try
                {
                    StreamSocket socket = new StreamSocket();
                    await socket.ConnectAsync(rfcommResult.Services[selectedService].ConnectionHostName, rfcommResult.Services[selectedService].ConnectionServiceName);
                    Console.WriteLine("Connected to service: " + rfcommResult.Services[selectedService].ServiceId.Uuid);

                    CancellationTokenSource source = new CancellationTokenSource();
                    CancellationToken cancelToken = source.Token;
                    Task listenOnChannel = new TaskFactory().StartNew(async () =>
                    {
                        while (true)
                        {
                            if (cancelToken.IsCancellationRequested)
                            {
                                return;
                            }
                            await ReadWrite.Read(socket, source);
                        }
                    }, cancelToken);

                    Console.ReadKey();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not connect to service " + e.Message);
                    Exit();
                }
            }
            else
            {
                Exit();
            }
        }

        static void Exit()
        {
            Console.ReadKey();
            Environment.Exit(1);
        }
    }

    static class ReadWrite
    {
        public static async Task Read(StreamSocket socket, CancellationTokenSource source)
        {
            IBuffer buffer = new Windows.Storage.Streams.Buffer(1024);
            uint bytesRead = 1024;

            IBuffer result = await socket.InputStream.ReadAsync(buffer, bytesRead, InputStreamOptions.Partial);
            await Write(socket, "OK");

            DataReader reader = DataReader.FromBuffer(result);
            var output = reader.ReadString(result.Length);

            if (output.Length != 0)
            {
                Console.WriteLine("Recieved :" + output.Replace("\r", " "));
                if (output.Contains("IPHONEACCEV"))
                {
                    try
                    {
                        var batteryCmd = output.Substring(output.IndexOf("IPHONEACCEV"));
                        Console.WriteLine("Battery level :" + (Int32.Parse(batteryCmd.Substring(batteryCmd.LastIndexOf(",") + 1)) + 1) * 10);
                        source.Cancel();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Could not retrieve " + e.Message);
                    }
                }
            }
        }

        public static async Task Write(StreamSocket socket, string str)
        {
            var bytesWrite = CryptographicBuffer.ConvertStringToBinary("\r\n" + str + "\r\n", BinaryStringEncoding.Utf8);
            await socket.OutputStream.WriteAsync(bytesWrite);
        }
    }
}
