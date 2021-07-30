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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;

namespace MyDevices.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            this.myNotifyIcon.TrayMouseDoubleClick += MyNotifyIcon_TrayMouseDoubleClick;

            DeviceMqtt mqtt = new DeviceMqtt();

            webView.Source = new Uri("https://www.baidu.com");
        }

        private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        void OpenWin_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        void QuitWin_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}