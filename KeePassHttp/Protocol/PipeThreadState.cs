using System.IO.Pipes;
using System.Threading;

namespace KeePassHttp.Protocol
{
    public class PipeThreadState
    {
        public Thread Thread { get; set; }
        public ManualResetEventSlim Closing { get; }

        public PipeThreadState(Thread t)
        {
            Closing = new ManualResetEventSlim(false);
            Thread = t;
        }
    }
}
