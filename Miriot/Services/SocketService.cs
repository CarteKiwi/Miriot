using Miriot.Model;
using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Miriot.Services
{
    public class SocketService
    {
        private List<RomeRemoteSystem> _remoteSystems;
        public IReadOnlyList<RomeRemoteSystem> RemoteSystems => _remoteSystems.ToList();

        public Action<RomeRemoteSystem> Added { get; set; }


        public async void Receiver()
        {
            var listenPort = 11011;
            var receiver = new UdpSocketReceiver();

            receiver.MessageReceived += (sender, args) =>
            {
                // get the remote endpoint details and convert the received data into a string
                var from = String.Format("{0}:{1}", args.RemoteAddress, args.RemotePort);
                var data = Encoding.UTF8.GetString(args.ByteData, 0, args.ByteData.Length);

                Debug.WriteLine("{0} - {1}", from, data);

                var msg = Environment.MachineName;
                var msgBytes = Encoding.UTF8.GetBytes(msg);

                receiver.SendToAsync(msgBytes, args.RemoteAddress, int.Parse(args.RemotePort));
            };

            // listen for udp traffic on listenPort
            await receiver.StartListeningAsync(listenPort);
        }

        public async void Client()
        {
            var port = 11011;
            var address = "127.0.0.1";

            var client = new UdpSocketClient();

            // convert our greeting message into a byte array
            var msg = "HELLO WORLD";
            var msgBytes = Encoding.UTF8.GetBytes(msg);

            // send to address:port, 
            // no guarantee that anyone is there 
            // or that the message is delivered.
            await client.SendToAsync(msgBytes, address, port);

            client.MessageReceived += (s, args) =>
            {
                var add = args.RemoteAddress;
                var name = Encoding.UTF8.GetString(args.ByteData, 0, args.ByteData.Length);

                Added?.Invoke(new RomeRemoteSystem(null)
                {
                    DisplayName = name
                });
            };
        }
    }
}
