using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteClient.Core
{
    public class AppSettings
    {
        public string? DBName { get; set; }
        public string? DBPWD { get; set; }
        public string? SSHPath { get; set; }
        public string? SSHArguments { get; set; }
        public string? SSHPrivateKeyArguments { get; set; }
        public string? SFTPPath { get; set; }
        public string? SFTPArguments { get; set; }
        public string? SFTPPrivateKeyArguments { get; set; }
        public string? PrivateKeyExtension { get; set; }
    }
}
