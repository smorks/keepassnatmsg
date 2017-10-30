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

        private const int BufferSize = 1024 * 1024;

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
                pts.WaitHandle.Set();
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

            pts.Server = new NamedPipeServerStream(_name, PipeDirection.InOut, Threads, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

            pts.Server.BeginWaitForConnection(HandleConnection, pts);
            pts.WaitHandle.Wait();

            pts.Server.Close();
            ThreadClosed(pts);
        }

        private void HandleConnection(IAsyncResult ar)
        {
            if (_active)
            {
                var pts = (PipeThreadState)ar.AsyncState;
                pts.Server.EndWaitForConnection(ar);

                var state = new PipeReadState(pts);

                while (!pts.WaitHandle.IsSet)
                {
                    var readAr = pts.Server.BeginRead(state.Data, 0, state.Data.Length, HandleRead, state);
                    readAr.AsyncWaitHandle.WaitOne();
                }
            }
        }

        private void HandleRead(IAsyncResult ar)
        {
            if (_active)
            {
                var state = (PipeReadState)ar.AsyncState;
                if (!state.ThreadState.WaitHandle.IsSet)
                {
                    var server = state.ThreadState.Server;
                    var bytes = server.EndRead(ar);
                    if (bytes > 0)
                    {
                        if (server.IsMessageComplete)
                        {
                            MessageReceived?.Invoke(this, new PipeMessageReceivedEventArgs(state.ThreadState, state.GetData(bytes)));
                            state.Reset();
                        }
                        else
                        {
                            state.AppendData(bytes);
                        }
                    }
                    else if (bytes == 0)
                    {
                        state.ThreadState.WaitHandle.Set();
                    }
                }
            }
        }

        private class PipeReadState
        {
            public PipeThreadState ThreadState { get; }
            public byte[] Data { get; private set; }
            private byte[] Partial { get; set; }

            public PipeReadState(PipeThreadState pts)
            {
                ThreadState = pts;
                Data = new byte[BufferSize];
                Partial = null;
            }

            public void AppendData(int dataLength)
            {
                Partial = GetData(dataLength);
                Data = new byte[BufferSize];
            }

            public byte[] GetData(int dataLength)
            {
                byte[] d;
                if (Partial == null)
                {
                    if (dataLength == Data.Length) return Data;
                    d = new byte[dataLength];
                    Array.Copy(Data, d, dataLength);
                }
                else
                {
                    d = new byte[Partial.Length + dataLength];
                    Array.Copy(Partial, d, Partial.Length);
                    Array.Copy(Data, 0, d, Partial.Length, dataLength);
                }
                return d;
            }

            public void Reset()
            {
                Partial = null;
            }
        }
    }

    public class PipeMessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public PipeThreadState ThreadState { get; set; }

        public PipeMessageReceivedEventArgs(PipeThreadState pts, byte[] data)
        {
            ThreadState = pts;
            Message = System.Text.Encoding.UTF8.GetString(data);
        }
    }
}
