using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChenduZhuanYeJiShuApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private LoginPage _LoginPage;

        public MainWindow()
        {
            InitializeComponent();

            _LoginPage = new LoginPage(this);

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GoToLoginPage();
        }

        public void GoToLoginPage()
        {
            this.MainFrame.Content = this._LoginPage;
        }
    }
}
