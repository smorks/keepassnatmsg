using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;

namespace KeePassHttp.Protocol
{
    public sealed class NamedPipeListener
    {
        private const int Threads = 5;
        private readonly string _name;
        private volatile bool _active;
        private readonly List<PipeThreadState> _threads;

        public event EventHandler<PipeMessageReceivedEventArgs> MessageReceived;

        public NamedPipeListener(string name)
        {
            _name = name;
            _threads = new List<PipeThreadState>();
        }

        public void Start()
        {
            _active = true;
            for (var i = 0; i < Threads; i++)
            {
                CreateAndRunThread();
            }
        }

        public void Stop()
        {
            _active = false;
            foreach (var pts in _threads)
            {
                pts.Closing.Set();
                pts.Thread.Join();
            }
        }

        private void CreateAndRunThread()
        {
            var pts = CreateThreadState(new Thread(Run));
            pts.Thread.Start(pts);
        }

        private PipeThreadState CreateThreadState(Thread t)
        {
            var pts = new PipeThreadState(t);
            _threads.Add(pts);
            return pts;
        }

        private void RunThreadClosed(object args)
        {
            var pts = (PipeThreadState)args;
            _threads.Remove(pts);
            pts.Thread.Join();
            pts = CreateThreadState(Thread.CurrentThread);
            Run(pts);
        }

        private void ThreadClosed(PipeThreadState pts)
        {
            if (_active)
            {
                var t = new Thread(RunThreadClosed);
                t.Start(pts);
            }
        }

        private void Run(object args)
        {
            var pts = (PipeThreadState)args;

            var server = new NamedPipeServerStream(_name, PipeDirection.InOut, Threads);

            server.WaitForConnection();

            while (_active && !pts.Closing.IsSet && server.IsConnected)
            {
                var hdr = new byte[4];
                var bytes = server.Read(hdr, 0, hdr.Length);
                if (bytes == hdr.Length)
                {
                    var dataLen = BitConverter.ToInt32(hdr, 0);
                    var data = new byte[dataLen];
                    bytes = server.Read(data, 0, data.Length);
                    if (bytes == data.Length)
                    {
                        MessageReceived?.Invoke(this, new PipeMessageReceivedEventArgs(new PipeWriter(server), data));
                    }
                }
            }

            server.Close();
            ThreadClosed(pts);
        }
    }

    public class PipeMessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public PipeWriter Writer { get; set; }

        public PipeMessageReceivedEventArgs(PipeWriter writer, byte[] data)
        {
            Writer = writer;
            Message = System.Text.Encoding.UTF8.GetString(data);
        }
    }
}
