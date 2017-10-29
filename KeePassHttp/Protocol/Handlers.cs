using KeePass.Plugins;
using KeePassHttp.Protocol.Action;
using KeePassHttp.Protocol.Crypto;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KeePassHttp.Protocol
{
    public sealed class Handlers
    {
        private Helper _crypto;
        private KeePassHttpExt _ext;
        private Dictionary<string, RequestHandler> _handlers;
        private IPluginHost _host;

        public delegate Response RequestHandler(Request req);

        public Handlers()
        {
            _crypto = new Helper();
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

        public RequestHandler GetHandler(string action) => _handlers[action];

        private Response GetDatabaseHash(Request req)
        {
            var reqMsg = _crypto.DecryptMessage(req);
            if (reqMsg == null) return null;
            var resp = req.GetResponse();
            var msg = GetResponseMessage();
            msg.Add("nonce", resp.Nonce);
            _crypto.EncryptMessage(resp, msg.ToString());
            return resp;
        }

        private Response TestAssociate(Request req)
        {
            var entry = _ext.GetConfigEntry(false);
            if (entry != null)
            {
                var msg = _crypto.DecryptMessage(req);
                var x = entry.Strings.First(e => e.Key.Equals(KeePassHttpExt.ASSOCIATE_KEY_PREFIX + msg.GetString("id")));
                var key = x.Value;
                var reqKey = msg.GetBytes("key");
                var id = msg.GetString("id");
                var dbKey = Convert.FromBase64String(key.ReadString());
                if (dbKey.SequenceEqual(reqKey) && !string.IsNullOrWhiteSpace(id))
                {
                    var resp = req.GetResponse();
                    var respMsg = GetResponseMessage();
                    respMsg.Add("id", id);
                    respMsg.Add("nonce", resp.Nonce);
                    _crypto.EncryptMessage(resp, respMsg.ToString());
                    return resp;
                }
            }
            return null;
        }

        private Response Associate(Request req)
        {
            var msg = _crypto.DecryptMessage(req);
            var keyBytes = msg.GetBytes("key");
            if (keyBytes.SequenceEqual(_crypto.ClientPublicKey))
            {
                var id = _ext.ShowConfirmAssociationDialog(msg.GetString("key"));
                var resp = req.GetResponse();
                var respMsg = GetResponseMessage();
                respMsg.Add("id", id);
                respMsg.Add("nonce", resp.Nonce);
                _crypto.EncryptMessage(resp, respMsg.ToString());
                return resp;
            }
            else
            {
                _ext.ShowNotification("Association Failed. Public Keys don't match.");
            }
            return null;
        }

        private Response GetErrorResponse(string action, string error)
        {
            var r = new Response(action);
            r.Remove("nonce");
            r.Add("error", error);
            return r;
        }

        private JsonBase GetResponseMessage()
        {
            return new JsonBase
            {
                {"hash", _ext.GetDbHash()},
                {"version", GetVersion()},
                {"success", "true"}
            };
        }

        private Response ChangePublicKeys(Request req)
        {
            var publicKey = req.GetString("publicKey");
            _crypto.ClientPublicKey = Convert.FromBase64String(publicKey);
            var pair = _crypto.GenerateKeyPair();
            var resp = req.GetResponse();
            resp.AddBytes("publicKey", pair.PublicKey);
            resp.Add("version", GetVersion());
            resp.Add("success", "true");
            //var respMsg = GetResponseMessage();
            //respMsg.Remove("hash");
            //_crypto.EncryptMessage(resp, respMsg.ToString());
            return resp;
        }

        private string GetVersion() => typeof(Handlers).Assembly.GetName().Version.ToString();

        private Response GetLogins(Request req)
        {
            var es = new EntrySearch(_crypto);
            return es.GetLoginsHandler(req, GetResponseMessage());
        }

        private Response SetLogin(Request req)
        {
            var eu = new EntryUpdate();
            var reqMsg = _crypto.DecryptMessage(req);
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

            var resp = req.GetResponse();
            var msg = GetResponseMessage();
            msg.Add("nonce", resp.Nonce);
            _crypto.EncryptMessage(resp, msg.ToString());
            return resp;
        }

        private Response GeneratePassword(Request req)
        {
            var resp = req.GetResponse();
            var msg = GetResponseMessage();
            msg.Add("entries", new JArray(_ext.GeneratePassword()));
            msg.Add("nonce", resp.Nonce);
            _crypto.EncryptMessage(resp, msg.ToString());
            return resp;
        }

        private Response LockDatabase(Request req)
        {
            var resp = req.GetResponse();
            var msg = GetResponseMessage();
            msg.Add("nonce", resp.Nonce);
            _crypto.EncryptMessage(resp, msg.ToString());
            _host.MainWindow.LockAllDocuments();
            return resp;
        }
    }
}
