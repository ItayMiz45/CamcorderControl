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

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private MainWindow MasterWindow;
        private User connectedUser;

        public MainPage(MainWindow window, User user)
        {
            InitializeComponent();

            MasterWindow = window;
            connectedUser = user;

            MasterWindow.LogoutButton.Visibility = Visibility.Visible;
            LoggedTextBlock.Text += connectedUser.Username;
        }
    }
}
