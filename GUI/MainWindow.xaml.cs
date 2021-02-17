﻿using System;
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
using System.Threading;
using System.IO.Pipes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Thread serverThread { get; set; }
        public User connectedUser;

        public MainWindow()
        {
            InitializeComponent();

            MainFrame.Content = new SigningPage(this);
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is MainPage) // main page has a thread to get frames, so close it when we leave it
            {
                CloseThreadWhenCantConnectToPipe(((MainPage)MainFrame.Content).serverThread);
            }


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

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {            
            if (MainFrame.Content is MainPage) // main page has a thread to get frames, so close it when we leave it
            {
                connectedUser = null;
                CloseThreadWhenCantConnectToPipe(((MainPage)MainFrame.Content).serverThread);
                MainFrame.Content = new SigningPage(this);
            }

            else if (MainFrame.Content is SettingsPage) // if logout is pressed and we are not in signing page, go to singing page
            {
                MainFrame.Content = new MainPage(this);
            }
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is MainPage)
            {
                CloseThreadWhenCantConnectToPipe(((MainPage)MainFrame.Content).serverThread);
            }
            MainFrame.Content = new SettingsPage(this);
        }

        public void LogoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(MainFrame.Content is MainPage)
            {
                CloseThreadWhenCantConnectToPipe(((MainPage)MainFrame.Content).serverThread);
            }

            if (!(MainFrame.Content is SigningPage)) // if logout is pressed and we are not in signing page, go to singing page
            {
                MainFrame.Content = new SigningPage(this);
            }
        }

        private void CloseThreadWhenCantConnectToPipe(Thread serverThread)
        {
            // if a client did not connect to the pipe, the easiest way to close the server is to create a fake connection
            using (NamedPipeClientStream npcs = new NamedPipeClientStream("CamcorderPipe"))
            {
                try
                {
                    npcs.Connect(100);
                    Thread.Sleep(50);
                }
                catch { } // client already connected, can't create a fake connection
            }

            serverThread.Abort(); // throw a ThreadAbortException in the thread to let it know it's time to die
        }

        
    }
}
