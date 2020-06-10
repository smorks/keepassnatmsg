using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace KeePassNatMsg.Protocol.Listener
{
    public sealed class NamedPipeListener : IListener
    {
        private const int BufferSize = 1024*1024;
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
                pts.Close();
            }
        }

        public void Write(string msg)
        {
            var pts = _threads.Find(x => x.Server.IsConnected);
            if (pts != null)
            {
                var pw = new PipeWriter(pts.Server);
                pw.Send(msg);
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
            pts.Close();
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
            var read = true;
            var pts = (PipeThreadState)args;
            NamedPipeServerStream server;

            server = new NamedPipeServerStream(_name, PipeDirection.InOut, Threads, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            pts.Server = server;

            try
            {
                server.WaitForConnection();

                while (_active && server.IsConnected && read)
                {
                    var buffer = new byte[BufferSize];
                    var bytes = server.Read(buffer, 0, buffer.Length);

                    if (bytes > 0)
                    {
                        var data = new byte[bytes];
                        Array.Copy(buffer, data, bytes);
                        MessageReceived?.BeginInvoke(this, new PipeMessageReceivedEventArgs(new PipeWriter(server), data), null, null);
                    }
                    else if (bytes == 0)
                    {
                        read = false;
                    }
                }
            }
            catch (IOException)
            {
            }

            ThreadClosed(pts);
        }
    }

    public class PipeMessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public IMessageWriter Writer { get; set; }

        public PipeMessageReceivedEventArgs(IMessageWriter writer, byte[] data)
        {
            Writer = writer;
            Message = System.Text.Encoding.UTF8.GetString(data);
        }
    }
}
