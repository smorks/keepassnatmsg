using KeePass.Plugins;
using KeePassHttp.Protocol.Action;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KeePassHttp.Protocol
{
    public sealed class Handlers
    {
        private KeePassHttpExt _ext;
        private Dictionary<string, RequestHandler> _handlers;
        private IPluginHost _host;

        public delegate Response RequestHandler(Request req);

        public Handlers()
        {
            _ext = KeePassHttpExt.ExtInstance;
            _host = KeePassHttpExt.HostInstance;
        }

        public void Initialize()
        {
            _handlers = new Dictionary<string, RequestHandler>
            {
                {Actions.GET_DATABASE_HASH, GetDatabaseHash},
                {Actions.TEST_ASSOCIATE, TestAssociate},
                {Actions.ASSOCIATE, Associate},
                {Actions.CHANGE_PUBLIC_KEYS, ChangePublicKeys},
                {Actions.GET_LOGINS, GetLogins},
                {Actions.SET_LOGIN, SetLogin},
                {Actions.GENERATE_PASSWORD, GeneratePassword},
                {Actions.LOCK_DATABASE, LockDatabase}
            };
        }

        public Response ProcessRequest(Request req)
        {
            var handler = GetHandler(req.Action);
            if (handler != null)
            {
                return handler.Invoke(req);
            }
            return GetErrorResponse(req.Action, ErrorType.IncorrectAction);
        }

        private RequestHandler GetHandler(string action) => _handlers.ContainsKey(action) ? _handlers[action] : null;

        private Response GetDatabaseHash(Request req)
        {
            if (req.TryDecrypt())
            {
                return req.GetResponse();
            }
            return GetErrorResponse(req.Action, ErrorType.CannotDecryptMessage);
        }

        private Response TestAssociate(Request req)
        {
            var entry = _ext.GetConfigEntry(false);
            if (entry != null)
            {
                if (req.TryDecrypt())
                {
                    var msg = req.Message;
                    var x = entry.Strings.FirstOrDefault(e => e.Key.Equals(KeePassHttpExt.ASSOCIATE_KEY_PREFIX + msg.GetString("id")));
                    var key = x.Value;
                    var reqKey = msg.GetBytes("key");
                    var id = msg.GetString("id");
                    var dbKey = Convert.FromBase64String(key.ReadString());
                    if (dbKey.SequenceEqual(reqKey) && !string.IsNullOrWhiteSpace(id))
                    {
                        var resp = req.GetResponse();
                        resp.Message.Add("id", id);
                        return resp;
                    }
                    return GetErrorResponse(req.Action, ErrorType.AssociationFailed);
                }
                return GetErrorResponse(req.Action, ErrorType.CannotDecryptMessage);
            }
            return GetErrorResponse(req.Action, ErrorType.AssociationFailed);
        }

        private Response Associate(Request req)
        {
            if (req.TryDecrypt())
            {
                var msg = req.Message;
                var keyBytes = msg.GetBytes("key");
                if (keyBytes.SequenceEqual(KeePassHttpExt.CryptoHelper.ClientPublicKey))
                {
                    var id = _ext.ShowConfirmAssociationDialog(msg.GetString("key"));
                    var resp = req.GetResponse();
                    resp.Message.Add("id", id);
                    return resp;
                }
                else
                {
                    _ext.ShowNotification("Association Failed. Public Keys don't match.");
                    return GetErrorResponse(req.Action, ErrorType.AssociationFailed);
                }
            }
            return GetErrorResponse(req.Action, ErrorType.CannotDecryptMessage);
        }

        private Response GetErrorResponse(string action, ErrorType error)
        {
            var r = new Response(action);
            r.Remove("nonce");
            r.Add("errorCode", (int)error);
            r.Add("error", Errors.GetErrorMessage(error));
            return r;
        }

        private Response ChangePublicKeys(Request req)
        {
            var crypto = KeePassHttpExt.CryptoHelper;
            var publicKey = req.GetString("publicKey");
            crypto.ClientPublicKey = Convert.FromBase64String(publicKey);
            var pair = crypto.GenerateKeyPair();
            var resp = req.GetResponse();
            resp.AddBytes("publicKey", pair.PublicKey);
            resp.Add("version", GetVersion());
            resp.Add("success", "true");
            return resp;
        }

        private string GetVersion() => typeof(Handlers).Assembly.GetName().Version.ToString();

        private Response GetLogins(Request req)
        {
            var es = new EntrySearch();
            return es.GetLoginsHandler(req);
        }

        private Response SetLogin(Request req)
        {
            if (req.TryDecrypt())
            {
                var eu = new EntryUpdate();
                var reqMsg = req.Message;
                var url = reqMsg.GetString("url");
                var uuid = reqMsg.GetString("uuid");
                var login = reqMsg.GetString("login");
                var pw = reqMsg.GetString("password");
                var submitUrl = reqMsg.GetString("submitUrl");

                if (string.IsNullOrEmpty(uuid))
                {
                    eu.CreateEntry(login, pw, url, submitUrl, null);
                }
                else
                {
                    eu.UpdateEntry(uuid, login, pw, url);
                }

                return req.GetResponse();
            }
            return GetErrorResponse(req.Action, ErrorType.CannotDecryptMessage);
        }

        private Response GeneratePassword(Request req)
        {
            var resp = req.GetResponse();
            var msg = resp.Message;
            msg.Add("entries", new JArray(_ext.GeneratePassword()));
            return resp;
        }

        private Response LockDatabase(Request req)
        {
            _host.MainWindow.LockAllDocuments();
            return req.GetResponse();
        }
    }
}
