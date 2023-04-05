using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace RemoteClient.Core
{
    public class MainWindowViewModel : ObservableObject
    {
        private AppSettings _settings;
        private List<ServerModel> allServerList;
        public MainWindowViewModel(AppSettings settings)
        {
            _settings = settings;
            allServerList = new List<ServerModel>();
            ServerList = new ObservableCollection<ServerModel>();
            OpenServerCommand = new RelayCommand(OpenServer);
            DeleteServerCommand = new RelayCommand(DeleteServer);

            var folder = Path.Combine(Environment.CurrentDirectory, "temp");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public ServerService ServerService
        {
            get => ServerServiceBuilder.GetInstance();
        }
        public void Init()
        {
            allServerList = ServerService.GetAllServers().ToList();
            FilterText = string.Empty;
        }


        private bool _startSSH = true;
        public bool StartSSH
        {
            get => _startSSH;
            set => SetProperty(ref _startSSH, value);
        }

        private bool _startSFTP = true;
        public bool StartSFTP
        {
            get => _startSFTP;
            set => SetProperty(ref _startSFTP, value);
        }

        private string _filterText;
        public string FilterText
        {
            get => _filterText;
            set
            {
                SetProperty(ref _filterText, value);
                if (allServerList.Count > 0)
                {
                    ServerList = new ObservableCollection<ServerModel>(allServerList
                    .Where(p => p.ServerName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                            || p.ServerAddress.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                    ));
                }
            }
        }
        private ServerModel _selectedItem;
        public ServerModel SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        private ObservableCollection<ServerModel> _serverList;
        public ObservableCollection<ServerModel> ServerList
        {
            get => _serverList;
            set => SetProperty(ref _serverList, value);
        }

        public ICommand DeleteServerCommand { get; }
        private void DeleteServer()
        {
            if (SelectedItem == null)
            {
                return;
            }
            ServerService.Delete(SelectedItem);
            Init();
        }

        public ICommand OpenServerCommand { get; }
        private void OpenServer()
        {
            if (SelectedItem == null)
            {
                return;
            }

            switch (SelectedItem.ServerType)
            {
                case ServerType.Linux:
                    StartLinux();
                    break;
                case ServerType.Windows:
                    StartWindows();
                    break;
                default:
                    break;
            }
        }

        private void StartWindows()
        {
            if (SelectedItem == null)
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
                pro.StandardInput.WriteLine($"cmdkey /generic:\"{SelectedItem.ServerAddress}\" /user:\"{SelectedItem.UserName}\" /pass:\"{SelectedItem.UserPassword}\"");
                pro.StandardInput.WriteLine($"mstsc /v:\"{SelectedItem.ServerAddress}\"");
                Thread.Sleep(2000);
                pro.StandardInput.WriteLine($"cmdkey /delete:\"{SelectedItem.ServerAddress}\"");
            }
        }

        private void StartLinux()
        {
            if (SelectedItem == null)
            {
                return;
            }
            if (!StartSSH && !StartSFTP)
            {
                return;
            }

            using (Process pro = new Process())
            {
                var fileName = string.Empty;
                if (SelectedItem.UsePrivateKey)
                {
                    fileName = Path.Combine(Environment.CurrentDirectory, "temp", SelectedItem.ServerName + "_" + SelectedItem.Id + _settings.PrivateKeyExtension);
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    var privateKey = SelectedItem.UserPassword;
                    File.AppendAllText(fileName, privateKey);
                }
                if (StartSSH)
                {
                    StartLinuxClient(pro
                        , _settings.SSHPath
                        , SelectedItem.UsePrivateKey ? _settings.SSHPrivateKeyArguments : _settings.SSHArguments
                        , fileName);
                }
                if (StartSFTP)
                {
                    StartLinuxClient(pro
                        , _settings.SFTPPath
                        , SelectedItem.UsePrivateKey ? _settings.SFTPPrivateKeyArguments : _settings.SFTPArguments
                        , fileName);
                }

            }
        }

        private void StartLinuxClient(Process pro, string? fileName, string? argumentsSetting, string keyFileName)
        {
            var arguments = ReplaceArgument(argumentsSetting, keyFileName);

            pro.StartInfo.FileName = fileName;
            pro.StartInfo.Arguments = arguments;
            pro.Start();
            pro.Close();
        }

        private string ReplaceArgument(string arguments, string keyFileName)
        {
            if (arguments == null)
            {
                return string.Empty;
            }
            return arguments.Replace("{Server.UserName}", SelectedItem.UserName)
                            .Replace("{Server.ServerAddress}", SelectedItem.ServerAddress)
                            .Replace("{Server.UserPassword}", SelectedItem.UserPassword)
                            .Replace("{keyFileName}", keyFileName);
        }
    }
}
