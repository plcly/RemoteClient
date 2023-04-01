using CommunityToolkit.Mvvm.DependencyInjection;
using RemoteClient.Core;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace RemoteClient
{
    /// <summary>
    /// AddOrUpdateServerView.xaml 的交互逻辑
    /// </summary>
    public partial class AddOrUpdateServerView : Window
    {
        public AddOrUpdateServerView()
        {
            InitializeComponent();

            DataContext = Ioc.Default.GetRequiredService<AddOrUpdateServerViewModel>();
        }

        public void CloseWindow()
        {
            this.Close();
        }


    }
}
