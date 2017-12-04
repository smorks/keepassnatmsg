namespace KeePassHttp.Protocol.Listener
{
    public interface IMessageWriter
    {
        void Send(string msg);
    }
}
