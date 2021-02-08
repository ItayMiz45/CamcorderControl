using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
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
        public Thread serverThread { get; set; }

        public MainPage(MainWindow window, User user)
        {
            InitializeComponent();

            MasterWindow = window;
            connectedUser = user;

            MasterWindow.LogoutButton.Visibility = Visibility.Visible;
            MasterWindow.Menu.Visibility = Visibility.Visible;
            LoggedTextBlock.Text += connectedUser.Username;

            byte[] byteData = File.ReadAllBytes(@".\no_image_available.jpeg");
            FrameImage.Source = GetBitmapImageFromBytes(byteData);

            serverThread = new Thread(new ThreadStart(StartImagePipe));
            serverThread.Start();
        }


        private void StartImagePipe()
        {
            // Open the named pipe.
            var server = new NamedPipeServerStream("CamcorderPipe");

            // Waiting for connection
            server.WaitForConnection();

            // Connected
            var br = new BinaryReader(server);
            var bw = new BinaryWriter(server);

            Exception closedException = null;

            while (true)
            {
                try
                {
                    int len = (int)br.ReadUInt32(); // Read string length

                    Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            FrameImage.Source = GetBitmapImageFromBytes(br.ReadBytes(len)); // Read image bytes
                        }
                        catch (Exception e) // when closing connection ReadByte will throw an exception, so we need to re-throw it after invoke
                        {
                            closedException = e;
                        }
                    });

                    if(closedException != null)
                    {
                        throw closedException;
                    }

                    var buf = Encoding.ASCII.GetBytes("roger!");     // Get ASCII byte array   
                    bw.Write((uint)buf.Length);                // Write string length
                    bw.Write(buf);                              // Write string
                }
                catch (Exception)
                {
                    // Client disconnected/ thread aborted
                    // if ThreadAbortException was caught, it will be raised again after the catch
                    br.Close();
                    bw.Close();

                    server.Dispose();
                    server.Close();
                    server = null;
                    break;
                }
            }

            
        }

        private static BitmapImage GetBitmapImageFromBytes(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
    }
}
