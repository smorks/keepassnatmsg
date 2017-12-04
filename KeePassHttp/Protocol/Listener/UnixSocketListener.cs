using Mono.Unix;
using System;
using System.Net.Sockets;
using System.Threading;

namespace KeePassHttp.Protocol.Listener
{
    public class UnixSocketListener : IListener
    {
        private const string SocketName = "kpxc_server";

        private UnixEndPoint _uep;
        private Socket _socket;
        private bool _active;
        private Thread _t;

        public event EventHandler<PipeMessageReceivedEventArgs> MessageReceived;

        public UnixSocketListener()
        {
            var path = $"/tmp/{SocketName}";
            var xdg = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
            if (!string.IsNullOrEmpty(xdg))
            {
                path = System.IO.Path.Combine(xdg, SocketName);
            }
            _uep = new UnixEndPoint(path);
        }

        public void Start()
        {
            _active = true;
            _t = new Thread(RunThread);
            _t.Start();
        }

        public void Stop()
        {
            _active = false;
            _socket.Close();
            _t.Join();
        }

        private void RunThread()
        {
            _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            _socket.Bind(_uep);
            _socket.Listen(5);

            while (_active)
            {
                var result = _socket.BeginAccept(ProcessConnection, null);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        private void ProcessConnection(IAsyncResult ar)
        {
            if (_active)
            {
                var s = _socket.EndAccept(ar);
                var buffer = new byte[1024 * 16];
                var bytes = s.Receive(buffer);
                if (bytes > 0)
                {
                    var data = new byte[bytes];
                    Array.Copy(buffer, data, bytes);
                    MessageReceived?.Invoke(this, new PipeMessageReceivedEventArgs(new SocketWriter(s), data));
                }
            }
        }
    }
}
