using Mono.Unix;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace KeePassNatMsg.Protocol.Listener
{
    public class UnixSocketListener : IListener, IDisposable
    {
        private const string SocketName = "kpxc_server";

        private UnixEndPoint _uep;
        private Socket _socket;
        private bool _active;
        private Thread _t;
        private CancellationTokenSource _cts;

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
            _cts = new CancellationTokenSource();
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
            _cts.Cancel();
            _socket.Close();
            _t.Join();
            DeleteSocketFile();
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
                _socket.Close();
                _cts.Dispose();
            }
            // free native resources
        }

        private void RunThread()
        {
            DeleteSocketFile();

            _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            _socket.Bind(_uep);
            _socket.Listen(5);

            while (_active)
            {
                try
                {
                    var ar = _socket.BeginAccept(SocketAccept, null);
                    ar.AsyncWaitHandle.WaitOne();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Socket Error: {ex}");
                }
            }
        }

        private void SocketAccept(IAsyncResult r)
        {
            if (r.IsCompleted && _active)
            {
                var s = _socket.EndAccept(r);
                var buffer = new byte[1024 * 16];
                var read = true;

                while (_active && s.Connected && read)
                {
                    var srs = new SocketReadState
                    {
                        Socket = s,
                        Data = buffer
                    };
                    var ar = s.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, SocketRead, srs);
                    try
                    {
                        srs.WaitHandle.Wait(_cts.Token);
                        read = srs.Active;
                    }
                    catch (OperationCanceledException)
                    {
                        read = false;
                    }
                }
            }
        }

        private void SocketRead(IAsyncResult r)
        {
            var srs = r.AsyncState as SocketReadState;
            if (r.IsCompleted)
            {
                var bytes = srs.Socket.EndReceive(r);
                if (bytes > 0)
                {
                    var data = new byte[bytes];
                    Array.Copy(srs.Data, data, bytes);
                    MessageReceived?.BeginInvoke(this, new PipeMessageReceivedEventArgs(new SocketWriter(srs.Socket), data), null, null);
                }
                else if (bytes == 0)
                {
                    srs.Active = false;
                }
            }
            srs.WaitHandle.Set();
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
