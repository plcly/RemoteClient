using CommunityToolkit.Mvvm.DependencyInjection;
using RemoteClient.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RemoteClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<MainWindowViewModel>();
            txtKey.Focus();
        }

        private void btnAddOrUpdate_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var name = btn.Name;

            switch (name)
            {
                case "btnAdd":
                    AddServer();
                    break;
                case "btnUpdate":
                    if (lstBoxServerList.SelectedItem == null)
                    {
                        MessageBox.Show("请先选择一行");
                        return;
                    }
                    UpdateServer(lstBoxServerList.SelectedItem as ServerModel);
                    break;
                default:
                    break;
            }
        }

        private void UpdateServer(ServerModel? serverModel)
        {
            AddOrUpdateServerView addOrUpdate = new AddOrUpdateServerView();
            var vm = addOrUpdate.DataContext as AddOrUpdateServerViewModel;
            vm.CloseWindow = addOrUpdate.CloseWindow;
            vm.SelectedItem = serverModel;
            addOrUpdate.ShowDialog();
            InitDataContext();
        }

        private void AddServer()
        {
            AddOrUpdateServerView addOrUpdate = new AddOrUpdateServerView();
            var vm = addOrUpdate.DataContext as AddOrUpdateServerViewModel;
            vm.SelectedItem = new ServerModel();
            vm.CloseWindow = addOrUpdate.CloseWindow;
            addOrUpdate.ShowDialog();
            InitDataContext();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ShowMainContainer();
        }

        private void InitDataContext()
        {
            var vm = DataContext as MainWindowViewModel;
            vm.Init();
        }

        private void pwd_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowMainContainer();
            }
        }

        private void ShowMainContainer()
        {
            try
            {
                ServerServiceBuilder.Init(txtKey.Password, txtIV.Password);
                InitDataContext();
                InitGrid.Visibility = Visibility.Hidden;
                MainContent.Visibility = Visibility.Visible;
                txtFilter.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            var files = Directory.GetFiles(System.IO.Path.Combine(Environment.CurrentDirectory, "temp"));
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
