using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.WebSockets
{
    /// <summary>
    /// To be added.
    /// </summary>
    public static class ClientWebSocketExtensions
    {
        /// <summary>
        /// To be added.
        /// </summary>
        /// <param name="clientWebSocket"></param>
        /// <returns></returns>
        public static bool IsConnected(this ClientWebSocket clientWebSocket) =>
            clientWebSocket != null && clientWebSocket.State == WebSocketState.Open;

        /// <summary>
        /// To be added.
        /// </summary>
        /// <param name="clientWebSocket"></param>
        /// <param name="message"></param>
        /// <param name="cancelationToken"></param>
        /// <returns></returns>
        public static Task SendStringAsync(this ClientWebSocket clientWebSocket, string message, CancellationToken cancelationToken)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(messageBytes);
            return clientWebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancelationToken);
        }

        /// <summary>
        /// To be added.
        /// </summary>
        /// <param name="clientWebSocket"></param>
        /// <param name="cancelationToken"></param>
        /// <returns></returns>
        public static async Task<string> ReadStringAsync(this ClientWebSocket clientWebSocket, CancellationToken cancelationToken)
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);
            var result = await clientWebSocket.ReceiveAsync(buffer, cancelationToken);
            var messageBytes = buffer.Skip(buffer.Offset).Take(result.Count).ToArray();
            return Encoding.UTF8.GetString(messageBytes);
        }
    }
}