using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RemoteClient.Core
{
    public class ServerService
    {
        private static readonly CipherMode cipherMode = CipherMode.CBC;
        private static readonly PaddingMode paddingMode = PaddingMode.PKCS7;
        private static readonly string VerifyCode = "RemoteClientVerifyCode";
        private LiteDBHelper _dbHelper;
        private string? _key;
        private string? _iv;
        public ServerService(string? key, string? iv)
        {
            if (!string.IsNullOrEmpty(iv) && iv.Length != 16)
            {
                throw new ArgumentException("Only 16-bit iv are accepted");
            }
            _key = key;
            _iv = iv;

            var settings = Ioc.Default.GetRequiredService<AppSettings>();
            _dbHelper = new LiteDBHelper(settings.DBPWD, settings.DBName);

            Verify();
        }

        private void Verify()
        {
            var encryptedVerifyCode = EncryptUtils.EncryptStringAES(VerifyCode, _key, cipherMode, _iv, paddingMode);
            var code = _dbHelper.GetOrInitVerifyCode(encryptedVerifyCode);
            if (encryptedVerifyCode != code)
            {
                throw new Exception("Key is not correct.");
            }
        }

        public IEnumerable<ServerModel> GetServers(string serverName)
        {
            var servers = _dbHelper.GetServers(serverName);
            foreach (var server in servers)
            {
                yield return DecryptServer(server);
            }
        }

        public IEnumerable<ServerModel> GetAllServers()
        {
            var servers = _dbHelper.GetAllServers();
            foreach (var server in servers)
            {
                yield return DecryptServer(server);
            }
        }

        public void InsertOrUpdateServer(ServerModel server)
        {
            if (server.Id == 0)
            {
                InsertServer(server);
            }
            else
            {
                UpdateServer(server);
            }
        }

        public int InsertServer(ServerModel server)
        {
            var encrypted = EncryptServer(server);
            return _dbHelper.Insert(encrypted);
        }

        public bool UpdateServer(ServerModel server)
        {
            var encrypted = EncryptServer(server);
            return _dbHelper.Update(encrypted);
        }

        public bool Delete(ServerModel server)
        {
            return _dbHelper.Delete(server);
        }

        private ServerModel DecryptServer(ServerModel server)
        {
            var decrypted = new ServerModel();
            decrypted.Id = server.Id;
            decrypted.ServerName = server.ServerName;
            decrypted.ServerType = server.ServerType;
            decrypted.UsePrivateKey = server.UsePrivateKey;
            decrypted.Sequence = server.Sequence;
            decrypted.ServerAddress = EncryptUtils.DecryptStringAES(server.ServerAddress, _key, cipherMode, _iv, paddingMode);
            decrypted.UserName = EncryptUtils.DecryptStringAES(server.UserName, _key, cipherMode, _iv, paddingMode);
            decrypted.UserPassword = EncryptUtils.DecryptStringAES(server.UserPassword, _key, cipherMode, _iv, paddingMode);
            return decrypted;
        }

        private ServerModel EncryptServer(ServerModel server)
        {
            var encrypted = new ServerModel();
            encrypted.Id = server.Id;
            encrypted.ServerName = server.ServerName;
            encrypted.ServerType = server.ServerType;
            encrypted.UsePrivateKey = server.UsePrivateKey;
            encrypted.Sequence = server.Sequence;
            encrypted.ServerAddress = EncryptUtils.EncryptStringAES(server.ServerAddress, _key, cipherMode, _iv, paddingMode);
            encrypted.UserName = EncryptUtils.EncryptStringAES(server.UserName, _key, cipherMode, _iv, paddingMode);
            encrypted.UserPassword = EncryptUtils.EncryptStringAES(server.UserPassword, _key, cipherMode, _iv, paddingMode);
            return encrypted;
        }
    }
}
