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
        private ServerHandle _serverHandle;
        public MainWindowViewModel(AppSettings settings)
        {
            _settings = settings;
            _serverHandle = new ServerHandle(_settings);
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

            _serverHandle.OpenServer(SelectedItem, StartSSH, StartSFTP);            
        }
    }
}
