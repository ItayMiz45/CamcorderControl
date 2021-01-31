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

using static GUI.SQLiteDataAccess.SQLiteErrorCodes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User connectedUser;

        public MainWindow()
        {
            InitializeComponent();            
        }

        private void MyMainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            //if the window is not maximized, maximize it. Otherwise, restore it
            if (Application.Current.MainWindow.WindowState != WindowState.Maximized)
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            else
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectedUser = SQLiteDataAccess.GetUser(UsernameTextbox.Text);
            }
            catch(InvalidOperationException) // user doesn't exist
            {
                MessageBox.Show($"User doesn't exists", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                User newUser = new User(UsernameTextbox.Text);
                SQLiteDataAccess.AddUser(newUser);
                MessageBox.Show("User added successfully", "Good job", MessageBoxButton.OK, MessageBoxImage.Information);
                connectedUser = newUser;
            }
            catch(SQLiteException err)
            {
                switch ((SQLiteDataAccess.SQLiteErrorCodes)err.ErrorCode)
                {
                    case SQLiteDataAccess.SQLiteErrorCodes.UniqueError:
                        MessageBox.Show($"User {UsernameTextbox.Text} already exists", "Error!",MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show($"Unknown SQL error", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }
    }
}
