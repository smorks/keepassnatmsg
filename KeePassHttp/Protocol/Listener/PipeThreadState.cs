using System.IO.Pipes;
using System.Threading;

namespace KeePassHttp.Protocol.Listener
{
    public class PipeThreadState
    {
        public Thread Thread { get; }
        public NamedPipeServerStream Server { get; set; }

        public PipeThreadState(Thread t)
        {
            Thread = t;
        }

        public void Close()
        {
            Server.Close();
            Thread.Join();
        }
    }
}
