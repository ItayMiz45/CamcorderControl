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
using System.Diagnostics;
using System.ComponentModel;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private MainWindow MasterWindow;
        public Thread serverThread { get; set; }

        public MainPage(MainWindow window)
        {
            InitializeComponent();

            MasterWindow = window;

            MasterWindow.GoBackButton.Visibility = Visibility.Visible;
            MasterWindow.Menu.Visibility = Visibility.Visible;
            LoggedTextBlock.Text += MasterWindow.connectedUser.Username;

            byte[] byteData = File.ReadAllBytes(@".\no_image_available.jpeg");
            FrameImage.Source = GetBitmapImageFromBytes(byteData);

            serverThread = new Thread(new ThreadStart(StartImagePipe));
            serverThread.Start();
            runClient();

        }

        private const string interpreterPath = @"D:\Itay\Python\CamcorderControl\venv\Scripts\python.exe";
        //private const string interpreterPath = @"python";
        private const string pythonScriptPath = @"D:\Itay\BigBigProject\GUI\GUI\ModelFiles\UseModel.py";
        //private const string pythonScriptPath = @"D:\Itay\Python\CamcorderControl\Test\test1.py";
        private const int PATH_UNUSED_DIRECTION = 23; //  'bin\Debug\netcoreapp3.1' len, need only the direction before this part (full path: 'G:\C#\runPySctript\cSharp\bin\Debug\netcoreapp3.1')

        private static void run_command(string cmd, bool hidden=false)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            if(hidden)
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + cmd;
            process.StartInfo = startInfo;
            process.Start();
        }

        private static void runClient()
        {
            run_command(@"..\..\ModelFiles\StartClient.bat", true);
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

                    uint isExec = br.ReadUInt32();
                    uint gest = br.ReadUInt32();
                    uint side = br.ReadUInt32();


                    if (isExec != 1)
                        continue;



                    Int64 gestId = SQLiteDataAccess.getGestureID($"{gest}", ((int)side) - 1);

                    Connector cnn = SQLiteDataAccess.GetConnectorByUserId(MasterWindow.connectedUser.UserId);

                    int idx = cnn.GesturesArray.IndexOf((int)gestId);
                    if (idx == -1)  // not found in list
                        continue;

                    int actionId = cnn.ActionsArray[idx];

                    Action action = SQLiteDataAccess.GetAction(actionId);

                    new Thread(() =>
                    {
                        run_command(action.Command);
                    }).Start();
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
