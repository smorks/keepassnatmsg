namespace KeePassHttp.Protocol.Action
{
    public class ErrorResponse : Response
    {
        public ErrorResponse(string action, ErrorType error) : base(action, false)
        {
            Remove("nonce");
            Add("errorCode", (int)error);
            Add("error", Errors.GetErrorMessage(error));
        }
    }
}
