namespace KeePassNatMsg.Protocol.Action
{
    public class ErrorResponse : Response
    {
        public ErrorResponse(Request req, ErrorType error) : base(req, false)
        {
            Remove("nonce");
            Add("errorCode", (int)error);
            Add("error", Errors.GetErrorMessage(error));
        }
    }
}
