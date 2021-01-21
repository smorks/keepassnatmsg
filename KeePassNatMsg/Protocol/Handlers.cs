using KeePass.Plugins;
using KeePassLib;
using KeePassNatMsg.Entry;
using KeePassNatMsg.Protocol.Action;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KeePassNatMsg.Protocol
{
    public sealed class Handlers
    {
        private KeePassNatMsgExt _ext;
        private Dictionary<string, RequestHandler> _handlers;
        private IPluginHost _host;
        private object _unlockLock;

        public delegate Response RequestHandler(Request req);

        public Handlers()
        {
            _ext = KeePassNatMsgExt.ExtInstance;
            _host = KeePassNatMsgExt.HostInstance;
            _unlockLock = new object();
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
                {Actions.LOCK_DATABASE, LockDatabase},
                {Actions.GET_DATABASE_GROUPS, GetDatabaseGroups},
                {Actions.CREATE_NEW_GROUP, CreateNewGroup},
                {Actions.GET_TOTP, GetTOTP}
            };
        }

        public Response ProcessRequest(Request req)
        {
            var handler = GetHandler(req.Action);
            if (handler != null)
            {
                if (handler != ChangePublicKeys && !UnlockDatabase(req.TriggerUnlock))
                {
                    return new ErrorResponse(req, ErrorType.DatabaseNotOpened);
                }

                return handler.Invoke(req);
            }
            return new ErrorResponse(req, ErrorType.IncorrectAction);
        }

        private bool UnlockDatabase(bool triggerUnlock)
        {
            lock (_unlockLock)
            {
                var config = new ConfigOpt(_host.CustomConfig);
                if (!_host.Database.IsOpen && config.UnlockDatabaseRequest && KeePass.UI.GlobalWindowManager.WindowCount == 0 && triggerUnlock)
                {
                    _host.MainWindow.Invoke(new System.Action(() => _host.MainWindow.OpenDatabase(_host.MainWindow.DocumentManager.ActiveDocument.LockedIoc, null, false)));
                }

                return _host.Database.IsOpen;
            }
        }

        private RequestHandler GetHandler(string action) => _handlers.ContainsKey(action) ? _handlers[action] : null;

        private Response GetDatabaseHash(Request req)
        {
            if (req.TryDecrypt())
            {
                return req.GetResponse();
            }
            return new ErrorResponse(req, ErrorType.CannotDecryptMessage);
        }

        private Response TestAssociate(Request req)
        {
            var entry = _ext.GetConfigEntry(false);
            if (entry != null)
            {
                if (req.TryDecrypt())
                {
                    var msg = req.Message;
                    var x = entry.Strings.FirstOrDefault(e => e.Key.Equals(KeePassNatMsgExt.AssociateKeyPrefix + msg.GetString("id")));
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
                    return new ErrorResponse(req, ErrorType.AssociationFailed);
                }
                return new ErrorResponse(req, ErrorType.CannotDecryptMessage);
            }
            return new ErrorResponse(req, ErrorType.AssociationFailed);
        }

        private Response Associate(Request req)
        {
            if (req.TryDecrypt())
            {
                var msg = req.Message;
                var keyBytes = msg.GetBytes("key");
                if (keyBytes.SequenceEqual(KeePassNatMsgExt.CryptoHelper.ClientPublicKey(req.ClientId)))
                {
                    var id = _ext.ShowConfirmAssociationDialog(msg.GetString("idKey"));
                    if (string.IsNullOrEmpty(id))
                    {
                        return new ErrorResponse(req, ErrorType.AssociationFailed);
                    }
                    var resp = req.GetResponse();
                    resp.Message.Add("id", id);
                    return resp;
                }
                else
                {
                    _ext.ShowNotification("Association Failed. Public Keys don't match.");
                    return new ErrorResponse(req, ErrorType.AssociationFailed);
                }
            }
            return new ErrorResponse(req, ErrorType.CannotDecryptMessage);
        }

        private Response ChangePublicKeys(Request req)
        {
            var crypto = KeePassNatMsgExt.CryptoHelper;
            var publicKey = req.GetString("publicKey");

            if (string.IsNullOrEmpty(publicKey))
                return new ErrorResponse(req, ErrorType.ClientPublicKeyNotReceived);

            var serverPublicKey = crypto.GenerateKeyPair(req.ClientId, Convert.FromBase64String(publicKey));
            var resp = req.GetResponse(false);
            resp.AddBytes("publicKey", serverPublicKey);
            resp.Add("version", KeePassNatMsgExt.GetVersion());
            resp.Add("success", "true");
            return resp;
        }

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
                var groupUuid = reqMsg.GetString("groupUuid");

                bool result;

                if (string.IsNullOrEmpty(uuid))
                {
                    result = eu.CreateEntry(login, pw, url, submitUrl, null, groupUuid);
                }
                else
                {
                    result = eu.UpdateEntry(uuid, login, pw, url);
                }

                var resp = req.GetResponse();

                resp.Message.Add("count", JValue.CreateNull());
                resp.Message.Add("entries", JValue.CreateNull());
                resp.Message.Add("error", result ? "success" : "error");

                return resp;
            }
            return new ErrorResponse(req, ErrorType.CannotDecryptMessage);
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

            _host.MainWindow.Invoke(new System.Action(() => _host.MainWindow.LockAllDocuments()));
            return req.GetResponse();
        }

        private Response GetDatabaseGroups(Request req)
        {
            var db = _ext.GetConnectionDatabase();

            if (db.RootGroup == null)
            {
                return new ErrorResponse(req, ErrorType.NoGroupsFound);
            }

            var root = new JObject
            {
                { "name", db.RootGroup.Name },
                { "uuid", db.RootGroup.Uuid.ToHexString() },
                { "children", GetGroupChildren(db.RootGroup) }
            };

            var resp = req.GetResponse();

            resp.Message.Add("groups", new JObject
            {
                { "groups", new JArray { root } }
            });

            return resp;
        }

        private JArray GetGroupChildren(PwGroup group)
        {
            var groups = new JArray();

            foreach(var grp in group.GetGroups(false))
            {
                groups.Add(new JObject
                {
                    { "name", grp.Name },
                    { "uuid", grp.Uuid.ToHexString() },
                    { "children", GetGroupChildren(grp) }
                });
            }

            return groups;
        }

        private Response CreateNewGroup(Request req)
        {
            if (!req.TryDecrypt())
                return new ErrorResponse(req, ErrorType.CannotDecryptMessage);

            var groupName = req.Message.GetString("groupName");

            var db = _ext.GetConnectionDatabase();

            var group = db.RootGroup.FindCreateSubTree(groupName, new[] { '/' }, true);

            if (group == null)
                return new ErrorResponse(req, ErrorType.CannotCreateNewGroup);

            var resp = req.GetResponse();

            resp.Message.Add("name", group.Name);
            resp.Message.Add("uuid", group.Uuid.ToHexString());

            return resp;
        }

        private Response GetTOTP(Request req)
        {
            if (!req.TryDecrypt())
                return new ErrorResponse(req, ErrorType.CannotDecryptMessage);

            var uuid = req.Message.GetString("uuid");

            var eut = new EntryTOTP();

            var totp = eut.GenerateFromUuid(uuid);
            if (string.IsNullOrEmpty(totp))
                return new ErrorResponse(req, ErrorType.NoLoginsFound);

            var resp = req.GetResponse();

            resp.Message.Add("totp", totp);

            return resp;

        }
    }
}
