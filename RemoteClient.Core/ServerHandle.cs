using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace RemoteClient.Core
{
    public class ServerHandle
    {
        private AppSettings _settings;
        public ServerHandle(AppSettings settings)
        {
            _settings = settings;
        }
        public void StartWindows(ServerModel selectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }
            using (Process pro = new Process())
            {
                pro.StartInfo.FileName = "cmd.exe";
                pro.StartInfo.RedirectStandardInput = true;
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.CreateNoWindow = true;
                pro.Start();
                pro.StandardInput.WriteLine($"cmdkey /generic:\"{selectedItem.ServerAddress}\" /user:\"{selectedItem.UserName}\" /pass:\"{selectedItem.UserPassword}\"");
                pro.StandardInput.WriteLine($"mstsc /v:\"{selectedItem.ServerAddress}\"");
                Thread.Sleep(2000);
                pro.StandardInput.WriteLine($"cmdkey /delete:\"{selectedItem.ServerAddress}\"");
            }
        }

        public void StartLinux(ServerModel selectedItem, bool startSSH, bool startSFTP)
        {
            if (selectedItem == null)
            {
                return;
            }
            if (!startSSH && !startSFTP)
            {
                return;
            }

            using (Process pro = new Process())
            {
                var fileName = string.Empty;
                if (selectedItem.UsePrivateKey)
                {
                    var folder = Path.Combine(Environment.CurrentDirectory, "temp");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    fileName = Path.Combine(folder, selectedItem.ServerName + "_" + selectedItem.Id + _settings.PrivateKeyExtension);
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    var privateKey = selectedItem.UserPassword;
                    File.AppendAllText(fileName, privateKey);
                }
                if (startSSH)
                {
                    StartLinuxClient(pro
                    , _settings.SSHPath
                    , selectedItem.UsePrivateKey ? _settings.SSHPrivateKeyArguments : _settings.SSHArguments
                        , fileName, selectedItem);
                }
                if (startSFTP)
                {
                    StartLinuxClient(pro
                    , _settings.SFTPPath
                    , selectedItem.UsePrivateKey ? _settings.SFTPPrivateKeyArguments : _settings.SFTPArguments
                    , fileName, selectedItem);
                }
            }
        }
        public void StartLinuxClient(Process pro, string? fileName, string? argumentsSetting, string keyFileName, ServerModel selectedItem)
        {
            var arguments = ReplaceArgument(argumentsSetting, keyFileName, selectedItem);

            pro.StartInfo.FileName = fileName;
            pro.StartInfo.Arguments = arguments;
            pro.Start();
            pro.Close();
        }

        private string ReplaceArgument(string arguments, string keyFileName, ServerModel selectedItem)
        {
            if (arguments == null)
            {
                return string.Empty;
            }
            return arguments.Replace("{Server.UserName}", selectedItem.UserName)
                            .Replace("{Server.ServerAddress}", selectedItem.ServerAddress)
                            .Replace("{Server.UserPassword}", selectedItem.UserPassword)
                            .Replace("{keyFileName}", keyFileName);
        }

        public void OpenServer(ServerModel selectedItem, bool startSSH, bool startSFTP)
        {
            if (selectedItem == null)
            {
                return;
            }

            switch (selectedItem.ServerType)
            {
                case ServerType.Linux:
                    StartLinux(selectedItem, startSSH, startSFTP);
                    break;
                case ServerType.Windows:
                    StartWindows(selectedItem);
                    break;
                default:
                    break;
            }
        }
    }
}
