using System;

namespace KeePassNatMsg.Protocol.Listener
{
    public interface IListener
    {
        void Start();
        void Stop();
        void Write(string msg);
        event EventHandler<PipeMessageReceivedEventArgs> MessageReceived;
    }
}
