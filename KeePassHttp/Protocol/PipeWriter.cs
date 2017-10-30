using System.IO.Pipes;

namespace KeePassHttp.Protocol
{
    public class PipeWriter
    {
        private readonly NamedPipeServerStream _server;
        private readonly System.Text.UTF8Encoding _utf8;

        public PipeWriter(NamedPipeServerStream server)
        {
            _server = server;
            _utf8 = new System.Text.UTF8Encoding(false);
        }

        public void Send(string msg)
        {
            var data = _utf8.GetBytes(msg);
            var sizeData = System.BitConverter.GetBytes(data.Length);
            _server.Write(sizeData, 0, sizeData.Length);
            _server.Write(data, 0, data.Length);
            _server.Flush();
        }
    }
}
