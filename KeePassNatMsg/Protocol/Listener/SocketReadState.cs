using System;
using System.Net.Sockets;
using System.Threading;

namespace KeePassNatMsg.Protocol.Listener
{
    internal sealed class SocketReadState : IDisposable
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                WaitHandle.Dispose();
            }
            // free native resources
        }
    }
}
