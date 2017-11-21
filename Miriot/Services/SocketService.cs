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
        private Timer _discoveryTimer;
        public Action<string, IPEndPoint> Discovered { get; internal set; }
        public Func<RemoteCommands, Task<string>> CommandReceived { get; internal set; }

        public async void BroadcastListener()
        {
            bool done = false;

            UdpClient udpClient = new UdpClient(11000);

            try
            {
                while (!done)
                {
                    IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    Debug.WriteLine("Waiting for broadcast");
                    udpClient.EnableBroadcast = true;
                    byte[] receiveBytes = udpClient.Receive(ref remoteIpEndPoint);

                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    // Uses the IPEndPoint object to determine which of these two hosts responded.
                    Debug.WriteLine("This is the message you received " +
                                                 returnData.ToString());
                    Debug.WriteLine("This message was sent from " +
                                                remoteIpEndPoint.Address.ToString() +
                                                " on their port number " +
                                                remoteIpEndPoint.Port.ToString());

                    string serializedData = string.Empty;

                    if (Enum.TryParse(returnData, out RemoteCommands cmd))
                    {
                        serializedData = await CommandReceived(cmd);
                    }

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
            _discoveryTimer.Change(0, Timeout.Infinite);
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

        internal Task<string> SendReceiveMessageAsync(IPEndPoint endPoint, string data)
        {
            using (var udpClient = new UdpClient())
            {
                var requestData = Encoding.ASCII.GetBytes(data);
                var serverEp = endPoint;

                udpClient.Connect(serverEp);
                udpClient.Send(requestData, requestData.Length);
                var serverResponseData = udpClient.Receive(ref serverEp);
                var serverResponse = Encoding.ASCII.GetString(serverResponseData);

                return Task.FromResult(serverResponse);
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
            _discoveryTimer = new Timer(OnBroadcast, null, 1000, 4000);
        }

        private void OnBroadcast(object state)
        {
            try
            {
                var udpClient = new UdpClient();
                var requestData = Encoding.ASCII.GetBytes("Miriot ?");
                var serverEp = new IPEndPoint(IPAddress.Any, 0);

                udpClient.EnableBroadcast = true;
                udpClient.Send(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, 11000));

                var serverResponseData = udpClient.Receive(ref serverEp);
                var serverResponse = Encoding.ASCII.GetString(serverResponseData);
                Debug.WriteLine("Received {0} from {1}", serverResponse, serverEp.Address.ToString());

                Discovered?.Invoke(serverResponse, serverEp);

                udpClient.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
