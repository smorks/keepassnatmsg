namespace KeePassNatMsg.Protocol
{
    public enum ErrorType
    {
        DatabaseNotOpened = 1,
        DatabaseHashNotReceived,
        ClientPublicKeyNotReceived,
        CannotDecryptMessage,
        TimeoutOrNotConnected,
        ActionCancelledOrDenied,
        CannotEncryptMessage,
        AssociationFailed,
        KeyChangeFailed,
        EncryptionKeyUnrecognized,
        NoSavedDatabasesFound,
        IncorrectAction,
        EmptyMessageReceived,
        NoUrlProvided,
        NoLoginsFound,
        NoGroupsFound,
        CannotCreateNewGroup
    }

    public static class Errors
    {
        public static string GetErrorMessage(ErrorType error)
        {
            switch (error)
            {
                case ErrorType.DatabaseNotOpened:
                    return "Database not opened";
                case ErrorType.DatabaseHashNotReceived:
                    return "Database hash not available";
                case ErrorType.ClientPublicKeyNotReceived:
                    return "Client public key not received";
                case ErrorType.CannotDecryptMessage:
                    return "Cannot decrypt message";
                case ErrorType.TimeoutOrNotConnected:
                    return "Timeout or cannot connect to KeePass";
                case ErrorType.ActionCancelledOrDenied:
                    return "Action cancelled or denied";
                case ErrorType.CannotEncryptMessage:
                    return "Cannot encrypt message or public key not found";
                case ErrorType.AssociationFailed:
                    return "Association failed";
                case ErrorType.KeyChangeFailed:
                    return "Key change was not successful";
                case ErrorType.EncryptionKeyUnrecognized:
                    return "Encryption key is not recognized";
                case ErrorType.NoSavedDatabasesFound:
                    return "No saved databases found";
                case ErrorType.IncorrectAction:
                    return "Incorrect action";
                case ErrorType.EmptyMessageReceived:
                    return "Empty message received";
                case ErrorType.NoUrlProvided:
                    return "No URL provided";
                case ErrorType.NoLoginsFound:
                    return "No Logins Found";
                case ErrorType.NoGroupsFound:
                    return "No Groups Found";
                case ErrorType.CannotCreateNewGroup:
                    return "Cannot Create New Group";
                default:
                    return error.ToString();
            }
        }
    }
}
