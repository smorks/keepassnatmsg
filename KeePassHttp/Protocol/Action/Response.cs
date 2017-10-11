using KeePassHttp.Protocol.Crypto;
using Newtonsoft.Json.Linq;

namespace KeePassHttp.Protocol.Action
{
    public class Response : JsonBase
    {
        public Response(string action)
        {
            Add("action", new JValue(action));
            AddBytes("nonce", Helper.GenerateNonce());
        }

        public byte[] Nonce => GetBytes("nonce");

        public void SetMessage(byte[] msg)
        {
            var existingMsg = this["message"];
            if (existingMsg == null)
            {
                AddBytes("message", msg);
            }
            else
            {
                this["message"] = System.Convert.ToBase64String(msg);
            }
        }

        public void SetMessage(JsonBase msg)
        {
            var existingMsg = this["message"];
            var b64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(msg.ToString()));
            if (existingMsg == null)
            {
                Add("message", b64);
            }
            else
            {
                this["message"] = b64;
            }
        }
    }
}
