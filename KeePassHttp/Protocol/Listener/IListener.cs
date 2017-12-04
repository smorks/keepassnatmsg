using System;

namespace KeePassHttp.Protocol.Listener
{
    public interface IListener
    {
        void Start();
        void Stop();
        event EventHandler<PipeMessageReceivedEventArgs> MessageReceived;
    }
}
