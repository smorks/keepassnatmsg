using System;

namespace KeePassNatMsg.Protocol.Listener
{
    public interface IListener
    {
        void Start();
        void Stop();
        event EventHandler<PipeMessageReceivedEventArgs> MessageReceived;
    }
}
