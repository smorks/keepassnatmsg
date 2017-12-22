namespace KeePassNatMsg.Protocol.Listener
{
    public interface IMessageWriter
    {
        void Send(string msg);
    }
}
