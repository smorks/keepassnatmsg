using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace KeePassHttp.Protocol
{
    public sealed class UdpListener
    {
        private Thread _thread;
        private UdpClient _client;
        private IPEndPoint _ep;
        private bool _active;
        private ManualResetEventSlim _gotData;
        private CancellationTokenSource _cts;
        private System.Text.UTF8Encoding _utf8 = new System.Text.UTF8Encoding(false);
        private int _port;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

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
                _thread.Start();
            }
        }

        public void Stop()
        {
            _active = false;
            _cts.Cancel();
            _thread.Join();
        }

        public void Send(string msg, IPEndPoint ep)
        {
            var data = _utf8.GetBytes(msg);
            _client.Send(data, data.Length, ep);
        }

        private void Run()
        {
            _active = true;
            _gotData = new ManualResetEventSlim(false);
            _cts = new CancellationTokenSource();
            _ep = new IPEndPoint(IPAddress.Any, _port);
            _client = new UdpClient(_ep);

            while (_active)
            {
                _gotData.Reset();
                _client.BeginReceive(ReceiveData, null);
                _gotData.Wait(_cts.Token);

                if (_cts.IsCancellationRequested) break;
            }

            _client.Close();
        }

        private void ReceiveData(IAsyncResult ar)
        {
            var ep = new IPEndPoint(IPAddress.Any, 0);
            var data = _client.EndReceive(ar, ref ep);
            var str = System.Text.Encoding.UTF8.GetString(data);
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(ep, str));
            _gotData.Set();
        }
    }

    public sealed class MessageReceivedEventArgs: EventArgs
    {
        public IPEndPoint From { get; private set; }
        public string Message { get; private set; }

        public MessageReceivedEventArgs(IPEndPoint from, string msg)
        {
            From = from;
            Message = msg;
        }
    }
}
