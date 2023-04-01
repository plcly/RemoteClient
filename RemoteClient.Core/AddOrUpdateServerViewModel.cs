using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteClient.Core
{
    public class AddOrUpdateServerViewModel : ObservableObject
    {
        public Action CloseWindow;
        public AddOrUpdateServerViewModel()
        {
            ConfirmCommand = new RelayCommand(Confirm);
        }

        public ServerService ServerService
        {
            get => ServerServiceBuilder.GetInstance();
        }

        private ServerModel? _selectedItem;
        public ServerModel? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        private List<string> _serverTypeList = new List<string>() { "Linux", "Windows" };
        public List<string> ServerTypeList
        {
            get => _serverTypeList;
        }

        private List<string> _usePrivateKeyList = new List<string>() { "False", "True" };
        public List<string> UsePrivateKeyList
        {
            get => _usePrivateKeyList;
        }

        private int _serverTypeIndex;
        public int ServerTypeIndex
        {
            get
            {
                if (SelectedItem != null)
                {
                    return (int)SelectedItem.ServerType;
                }
                return 0;
            }
            set
            {
                SetProperty(ref _serverTypeIndex, value);
                if (Enum.IsDefined(typeof(ServerType), _serverTypeIndex))
                {
                    SelectedItem.ServerType = (ServerType)_serverTypeIndex;
                }
            }
        }

        private int _usePrivateKeyIndex;
        public int UsePrivateKeyIndex
        {
            get
            {
                if (SelectedItem != null)
                {
                    return SelectedItem.UsePrivateKey ? 1 : 0;
                }
                return 0;
            }
            set
            {
                SetProperty(ref _usePrivateKeyIndex, value);
                SelectedItem.UsePrivateKey = _usePrivateKeyIndex == 0 ? false : true;
            }
        }

        public ICommand ConfirmCommand { get; }
        private void Confirm()
        {
            if (SelectedItem != null)
            {
                ServerService.InsertOrUpdateServer(SelectedItem);
                CloseWindow();
            }
        }
    }
}
