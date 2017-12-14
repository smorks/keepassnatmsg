using Mono.Unix;
using System;
using System.Diagnostics;
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
            DeleteSocketFile();
        }

        private void RunThread()
        {
            _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            _socket.Bind(_uep);
            _socket.Listen(5);

            while (_active)
            {
                try
                {
                    ReadLoop(_socket.Accept());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Socket Error: {ex}");
                }
            }
        }

        private void ReadLoop(Socket s)
        {
            var buffer = new byte[1024 * 16];

            while (_active && s.Connected)
            {
                var bytes = s.Receive(buffer);
                if (bytes > 0)
                {
                    var data = new byte[bytes];
                    Array.Copy(buffer, data, bytes);
                    MessageReceived?.BeginInvoke(this, new PipeMessageReceivedEventArgs(new SocketWriter(s), data), null, null);
                }
            }
        }

        private void DeleteSocketFile()
        {
            if (System.IO.File.Exists(_uep.Filename))
            {
                try
                {
                    System.IO.File.Delete(_uep.Filename);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error Deleting Socket File ({_uep.Filename}): {ex}");
                }
            }
        }
    }
}
