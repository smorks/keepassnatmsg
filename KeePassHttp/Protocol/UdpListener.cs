using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace KeePassHttp.Protocol
{
    public sealed class UdpListener
    {
        private readonly Thread _thread;
        private UdpClient _client;
        private IPEndPoint _ep;
        private volatile bool _active;
        private readonly System.Text.UTF8Encoding _utf8 = new System.Text.UTF8Encoding(false);
        private readonly int _port;

        public event EventHandler<UdpMessageReceivedEventArgs> MessageReceived;

        public UdpListener(int port)
        {
            _port = port;
            _thread = new Thread(new ThreadStart(Run))
            {
                Name = $"{GetType().Name}_Thread"
            };
        }

        public void Start()
        {
            if (_thread.ThreadState == ThreadState.Unstarted)
            {
                _active = true;
                _thread.Start();
            }
        }

        public void Stop()
        {
            _active = false;
            _client.Close();
            _thread.Join();
        }

        public void Send(string msg, IPEndPoint ep)
        {
            var data = _utf8.GetBytes(msg);
            _client.Send(data, data.Length, ep);
        }

        private void Run()
        {
            _ep = new IPEndPoint(IPAddress.Any, _port);
            _client = new UdpClient(_ep);

            while (_active)
            {
                var ar = _client.BeginReceive(ReceiveData, null);
                ar.AsyncWaitHandle.WaitOne();
            }
        }

        private void ReceiveData(IAsyncResult ar)
        {
            if (_active)
            {
                var ep = new IPEndPoint(IPAddress.Any, 0);
                var data = _client.EndReceive(ar, ref ep);
                var str = System.Text.Encoding.UTF8.GetString(data);
                MessageReceived?.Invoke(this, new UdpMessageReceivedEventArgs(ep, str));
            }
        }
    }

    public sealed class UdpMessageReceivedEventArgs: EventArgs
    {
        public IPEndPoint From { get; private set; }
        public string Message { get; private set; }

        public UdpMessageReceivedEventArgs(IPEndPoint from, string msg)
        {
            From = from;
            Message = msg;
        }
    }
}
