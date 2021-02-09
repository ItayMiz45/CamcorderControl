using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
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
using System.Data.Sql;
using System.Data.SQLite;

namespace GUI
{
    /// <summary>
    /// Interaction logic for SigningPage.xaml
    /// </summary>
    public partial class SigningPage : Page
    {
        private MainWindow MasterWindow;

        public SigningPage(MainWindow window)
        {
            InitializeComponent();
            MasterWindow = window;

            MasterWindow.GoBackButton.Visibility = Visibility.Hidden;
            MasterWindow.Menu.Visibility = Visibility.Hidden;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MasterWindow.connectedUser = SQLiteDataAccess.GetUser(UsernameTextbox.Text);
                MasterWindow.MainFrame.Content = new MainPage(MasterWindow);
                
            }
            catch (InvalidOperationException) // user doesn't exist
            {
                MessageBox.Show($"User doesn't exists", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MasterWindow.connectedUser = new User(UsernameTextbox.Text);
                SQLiteDataAccess.AddUser(MasterWindow.connectedUser);
                MasterWindow.MainFrame.Content = new MainPage(MasterWindow);
            }
            catch (SQLiteException err)
            {
                switch ((SQLiteDataAccess.SQLiteErrorCodes)err.ErrorCode)
                {
                    case SQLiteDataAccess.SQLiteErrorCodes.UniqueError:
                        MessageBox.Show($"User {UsernameTextbox.Text} already exists", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show($"Unknown SQL error", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }
    }
}
