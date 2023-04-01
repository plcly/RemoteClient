using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RemoteClient.Core
{
    public class LiteDBHelper
    {
        private string _dbName = "server.db";
        private ConnectionString _connection;
        public LiteDBHelper(string? dbPwd, string? dbName = null)
        {
            if (dbName != null)
            {
                _dbName = dbName;
            }
            var path = Path.Combine(Environment.CurrentDirectory, _dbName);
            _connection = new ConnectionString
            {
                Filename = path,
                Password = dbPwd,
            };
            using (var db = new LiteDatabase(_connection))
            {
                var col = db.GetCollection<ServerModel>();
            }
        }

        public string GetOrInitVerifyCode(string verifyCode)
        {
            using (var db = new LiteDatabase(_connection))
            {
                var col = db.GetCollection<MyVerify>();
                var f = col.FindAll().FirstOrDefault();
                if (f == null)
                {
                    var myVerify = new MyVerify { VerifyCode = verifyCode };
                    col.Insert(myVerify);
                }
                f = col.FindAll().FirstOrDefault();
                return f.VerifyCode;
            }
        }

        public IEnumerable<ServerModel> GetServers(string serverName)
        {
            using (var db = new LiteDatabase(_connection))
            {
                var col = db.GetCollection<ServerModel>();
                return col.Find(p => p.ServerName.Contains(serverName)).ToList();
            }
        }

        public int Insert(ServerModel model)
        {
            using (var db = new LiteDatabase(_connection))
            {
                var col = db.GetCollection<ServerModel>();
                return col.Insert(model);
            }
        }

        public bool Update(ServerModel model)
        {
            using (var db = new LiteDatabase(_connection))
            {
                var col = db.GetCollection<ServerModel>();
                if (col.Exists(p => p.Id == model.Id))
                {
                    return col.Update(model);
                }
                return false;
            }
        }

        public bool Delete(ServerModel model)
        {
            using (var db = new LiteDatabase(_connection))
            {
                var col = db.GetCollection<ServerModel>();
                if (col.Exists(p => p.Id == model.Id))
                {
                    return col.Delete(model.Id);
                }
                return false;
            }
        }

        public IEnumerable<ServerModel> GetAllServers()
        {
            using (var db = new LiteDatabase(_connection))
            {
                return db.GetCollection<ServerModel>().FindAll().OrderBy(p => p.Sequence).ToList();
            }
        }
    }
}
