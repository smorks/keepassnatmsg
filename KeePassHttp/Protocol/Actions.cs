
namespace KeePassHttp.Protocol
{
    public sealed class Actions
    {
        public const string GET_DATABASE_HASH = "get-databasehash";
        public const string ASSOCIATE = "associate";
        public const string TEST_ASSOCIATE = "test-associate";
        public const string GET_LOGINS = "get-logins";
        public const string SET_LOGIN = "set-login";
        public const string GENERATE_PASSWORD = "generate-password";
        public const string CHANGE_PUBLIC_KEYS = "change-public-keys";
        public const string LOCK_DATABASE = "lock-database";
    }
}
