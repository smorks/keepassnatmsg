using System.IO.Pipes;
using System.Threading;

namespace KeePassHttp.Protocol
{
    public class PipeThreadState
    {
        public Thread Thread { get; set; }
        public ManualResetEventSlim WaitHandle { get; }
        public NamedPipeServerStream Server { get; set; }
        private readonly System.Text.UTF8Encoding _utf8 = new System.Text.UTF8Encoding(false);

        public PipeThreadState(Thread t)
        {
            WaitHandle = new ManualResetEventSlim(false);
            Thread = t;
        }

        public void Send(string msg)
        {
            var data = _utf8.GetBytes(msg);
            Server.Write(data, 0, data.Length);
        }
    }
}
