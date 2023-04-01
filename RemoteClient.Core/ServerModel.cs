using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteClient.Core
{
    public class ServerModel
    {
        public int Id { get; set; }
        public string? ServerName { get; set; }
        public string? ServerAddress { get; set; }
        public string? UserName { get; set; }
        public string? UserPassword { get; set; }
        public ServerType ServerType { get; set; }
        public int Sequence { get; set; }
        public bool UsePrivateKey { get; set; }
    }

    public enum ServerType
    {
        Linux =0,
        Windows =1
    }

    public class MyVerify
    {
        public int Id { get; set; }
        public string VerifyCode { get; set; }
    }
}
