using System.Net.Sockets;
using System.Threading;

namespace KeePassNatMsg.Protocol.Listener
{
    internal sealed class SocketReadState
    {
        public Socket Socket;
        public bool Active;
        public byte[] Data;
        public ManualResetEventSlim WaitHandle;

        public SocketReadState()
        {
            WaitHandle = new ManualResetEventSlim(false);
            Active = true;
        }
    }
}
