using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Miriot.Core.ViewModels;
using Miriot.Model;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Miriot.Services
{
    public class SocketService
    {
        private const int _port = 11100;
        private Timer _discoveryTimer;
        private CancellationTokenSource _canceller;

        public Action<RomeRemoteSystem> Discovered { get; internal set; }
        public Func<RemoteParameter, Task<string>> CommandReceived { get; internal set; }

        public void ConnectTcp(String server, String message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 13000;
                TcpClient client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }

        public static void ListenTcp()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        public async void BroadcastListener()
        {
            bool done = false;

            UdpClient udpClient = new UdpClient(_port);

            try
            {
                while (!done)
                {
                    IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    Debug.WriteLine("Waiting for broadcast");
                    udpClient.EnableBroadcast = true;

                    byte[] receiveBytes = udpClient.Receive(ref remoteIpEndPoint);

                    Debug.WriteLine("Received bytes...");

                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    // Uses the IPEndPoint object to determine which of these two hosts responded.
                    Debug.WriteLine("This is the message you received " +
                                                 returnData.ToString());
                    Debug.WriteLine("This message was sent from " +
                                                remoteIpEndPoint.Address.ToString() +
                                                " on their port number " +
                                                remoteIpEndPoint.Port.ToString());

                    string serializedData = string.Empty;

                    var parameter = JsonConvert.DeserializeObject<RemoteParameter>(returnData);

                    serializedData = await CommandReceived(parameter);

                    if (!string.IsNullOrEmpty(serializedData))
                    {
                        Byte[] sendBytes = Encoding.ASCII.GetBytes(serializedData);
                        udpClient.Send(sendBytes, sendBytes.Length, remoteIpEndPoint);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            finally
            {
                udpClient.Close();
            }
        }

        public void StopBroadcasting()
        {
            // Stop broadcasting
            _canceller.Cancel();
        }

        internal void SendMessage(IPEndPoint endPoint, string data)
        {
            using (var udpClient = new UdpClient())
            {
                var requestData = Encoding.ASCII.GetBytes(data);
                var serverEp = endPoint;

                udpClient.Connect(serverEp);
                udpClient.Send(requestData, requestData.Length);
            }
        }

        internal async Task<string> SendReceiveMessageAsync(IPEndPoint endPoint, string data)
        {
            using (var udpClient = new UdpClient())
            {
                var requestData = Encoding.ASCII.GetBytes(data);
                var serverEp = endPoint;

                udpClient.Connect(serverEp);
                await udpClient.SendAsync(requestData, requestData.Length);
                var serverResponseData = await udpClient.ReceiveAsync();
                var serverResponse = Encoding.ASCII.GetString(serverResponseData.Buffer);

                return serverResponse;
            }
        }

        private static bool IsLocalIp(IPAddress address)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            foreach (var ip in ipHostInfo.AddressList)
            {
                if (ip.ToString() == address.ToString())
                    return true;
            }

            return false;
        }

        public void Broadcast()
        {
            _canceller = new CancellationTokenSource();
            Task.Run(() => OnBroadcast(_canceller));
        }

        private async void OnBroadcast(CancellationTokenSource cancellationToken)
        {
            try
            {
                var parameter = new RemoteParameter() { Command = RemoteCommands.MiriotDiscovery };

                while (!cancellationToken.IsCancellationRequested)
                {
                    var udpClient = new UdpClient();
                    var requestData = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(parameter));
                    var serverEp = new IPEndPoint(IPAddress.Any, 0);

                    udpClient.EnableBroadcast = true;
                    udpClient.Send(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, _port));

                    var serverResponseData = udpClient.Receive(ref serverEp);
                    var serverResponse = Encoding.ASCII.GetString(serverResponseData);

                    var system = JsonConvert.DeserializeObject<RomeRemoteSystem>(serverResponse);
                    system.EndPoint = serverEp;

                    Debug.WriteLine("Received {0} from {1}", system.DisplayName, system.EndPoint.Address + ":" + system.EndPoint.Port);

                    Discovered?.Invoke(system);

                    udpClient.Close();

                    await Task.Delay(1000);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
