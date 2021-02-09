using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private MainWindow MasterWindow;

        public SettingsPage(MainWindow window)
        {
            InitializeComponent();

            MasterWindow = window;
            ChangeUsernameTextbox.Text = MasterWindow.connectedUser.Username;

            List<Gesture> gestures = SQLiteDataAccess.GetGestures();
            SolidColorBrush colorBrush = new SolidColorBrush(Color.FromRgb(149, 149, 149)); // #959595

            List<Action> allActions = SQLiteDataAccess.GetActions();

            Connector userConnector = SQLiteDataAccess.GetConnectorByUserId(MasterWindow.connectedUser.UserId);

            var allCommands = new ObservableCollection<string>(allActions.Select(o => o.Command));

            ComboBox comboBox;
            DockPanel dockPanel;
            TextBlock gestTxtBlock;
            Gesture gesture;

            for (int i = 0; i < gestures.Count; i++)
            {
                gesture = gestures[i];

                int indexInActionArray = userConnector.GesturesArray.FindIndex(g => g == gesture.GestureId);
                int index = -1;

                if (indexInActionArray >= 0)
                {
                    int actionId = userConnector.ActionsArray[indexInActionArray];
                    
                    for (index = 0; index < allActions.Count; index++)
                    {
                        if (allActions[index].ActionId == actionId)
                        {
                            break;
                        }
                    }
                }
                

                gestTxtBlock = new TextBlock();
                gestTxtBlock.Text = gesture.ToString();
                gestTxtBlock.Foreground = colorBrush;
                gestTxtBlock.FontSize = 17;

                comboBox = new ComboBox();
                comboBox.SelectedIndex = index;
                comboBox.Height = 20;
                comboBox.Width = 125;
                comboBox.HorizontalAlignment = HorizontalAlignment.Right;
                comboBox.VerticalAlignment = VerticalAlignment.Top;
                comboBox.Margin = new Thickness(0, 5, 0, 0);
                comboBox.ItemsSource = allCommands;

                dockPanel = new DockPanel();
                dockPanel.Margin = new Thickness(40, 30 * i, 0, 0);
                dockPanel.Children.Add(gestTxtBlock);
                dockPanel.Children.Add(comboBox);
                Grid.SetRow(dockPanel, 5);

                SettingsGrid.Children.Add(dockPanel);
            }
        }
    }
}
