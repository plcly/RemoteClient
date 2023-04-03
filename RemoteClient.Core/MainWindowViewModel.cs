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
            using (Process pro = new Process())
            {
                var argument = string.Empty;
                var sftpArgument = string.Empty;
                if (SelectedItem.UsePrivateKey)
                {
                    var fileName = Path.Combine(Environment.CurrentDirectory, "temp", SelectedItem.ServerName + "_" + SelectedItem.Id + _settings.PrivateKeyExtension);
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    var privateKey = SelectedItem.UserPassword;
                    File.AppendAllText(fileName, privateKey);

                    argument = ReplaceArgument(_settings.SSHPrivateKeyArguments).Replace("{fileName}", fileName);
                    if (StartSFTP && !string.IsNullOrEmpty(_settings.SFTPPath))
                    {
                        sftpArgument = ReplaceArgument(_settings.SFTPPrivateKeyArguments).Replace("{fileName}", fileName);
                    }
                }
                else
                {
                    argument = ReplaceArgument(_settings.SSHArguments);
                    if (StartSFTP && !string.IsNullOrEmpty(_settings.SFTPPath))
                    {
                        sftpArgument = ReplaceArgument(_settings.SFTPArguments);
                    }
                }

                ProcessStartInfo p1 = new ProcessStartInfo();
                p1.FileName = _settings.SSHPath;
                p1.Arguments = argument;
                pro.StartInfo = p1;
                pro.Start();
                pro.Close();
                if (StartSFTP && !string.IsNullOrEmpty(_settings.SFTPPath))
                {
                    ProcessStartInfo p2 = new ProcessStartInfo();
                    p2.FileName = _settings.SFTPPath;
                    p2.Arguments = sftpArgument;
                    pro.StartInfo = p2;
                    pro.Start();
                    pro.Close();
                }
            }
        }


        private string ReplaceArgument(string arguments)
        {
            if (arguments == null)
            {
                return string.Empty;
            }
            return arguments.Replace("{Server.UserName}", SelectedItem.UserName)
                            .Replace("{Server.ServerAddress}", SelectedItem.ServerAddress)
                            .Replace("{Server.UserPassword}", SelectedItem.UserPassword);
        }
    }
}
