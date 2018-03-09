using Miriot.Model;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
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
