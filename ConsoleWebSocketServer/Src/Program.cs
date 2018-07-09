using System;
using System.Net;
using System.Net.Sockets;

using Fleck;

namespace ConsoleWebSocketServer.Src
{
    class Program
    {
        static void Main(string[] args)
        {
            var localAddress = GetLocalAddress();
            if (!string.IsNullOrWhiteSpace(localAddress))
            {
                var serverLocation = GetServerLocation(localAddress, 8181);
                using (var server = new WebSocketServer(serverLocation))
                {
                    server.Start(Config);
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Local address not found");
            }
        }

        public static void Config(IWebSocketConnection socket)
        {
            socket.OnOpen = OnOpen;
            socket.OnClose = OnClose;
            socket.OnMessage = (message) => HandleMessage(socket, message);
        }

        public static void OnOpen() => Console.WriteLine(nameof(OnOpen));
        public static void OnClose() => Console.WriteLine(nameof(OnClose));

        public static void HandleMessage(IWebSocketConnection socket, string message)
        {
            Console.WriteLine(message);
            socket.Send(message);
        }

        private static string GetLocalAddress()
        {
            foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (address.AddressFamily == AddressFamily.InterNetwork && address.ToString().StartsWith("10"))
                {
                    return address.ToString();
                }
            }
            return null;
        }

        private static string GetServerLocation(string ip, ushort port) => $"ws://{ip}:{port}";
    }
}