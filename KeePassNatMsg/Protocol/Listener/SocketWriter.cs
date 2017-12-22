using System.Net.Sockets;

namespace KeePassNatMsg.Protocol.Listener
{
    public class SocketWriter : IMessageWriter
    {
        private readonly Socket _socket;
        private readonly System.Text.UTF8Encoding _utf8;

        public SocketWriter(Socket socket)
        {
            _socket = socket;
            _utf8 = new System.Text.UTF8Encoding(false);
        }

        public void Send(string msg)
        {
            var data = _utf8.GetBytes(msg);
            _socket.Send(data);
        }
    }
}
