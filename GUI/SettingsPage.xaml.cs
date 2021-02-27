using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
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

        private List<ComboBox> comboBoxes = new List<ComboBox>();
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

                comboBoxes.Add(comboBox);

                dockPanel = new DockPanel();
                dockPanel.Margin = new Thickness(40, 30 * i, 0, 0);
                dockPanel.Children.Add(gestTxtBlock);
                dockPanel.Children.Add(comboBox);
                Grid.SetRow(dockPanel, 5);

                SettingsGrid.Children.Add(dockPanel);
            }
        }

        private void ChangeUsernameButton_Click(object sender, RoutedEventArgs e)
        {
            string newUsername = ChangeUsernameTextbox.Text;
            Int64 userID = MasterWindow.connectedUser.UserId;
            try
            {
                SQLiteDataAccess.ChangeUserName(userID, newUsername);
                MasterWindow.connectedUser.Username = newUsername;
            }
            catch (SQLiteException err)
            {
                switch ((SQLiteDataAccess.SQLiteErrorCodes)err.ErrorCode)
                {
                    case SQLiteDataAccess.SQLiteErrorCodes.UniqueError:
                        MessageBox.Show($"User {newUsername} already exists", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show($"Unknown SQL error", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            Int64 userID = MasterWindow.connectedUser.UserId;
            SQLiteDataAccess.DeleteUser(userID);
            MasterWindow.LogoutMenuItem_Click(sender, e);
        }

        private void ChangeConnectors_Click(object sender, RoutedEventArgs e)
        {
            string actionsArray = "";
            string gesturesArray = "";
            string newActionsArray = "";
            string newGesturesArray = "";
            int i = 0;

            foreach (ComboBox comboBox in comboBoxes)
            {
                i++;
                actionsArray += SQLiteDataAccess.getActionID(comboBox.SelectedItem.ToString()).ToString() + ',';
                gesturesArray += i.ToString() + ',';

            }
            for(int j = 0; j < actionsArray.Length - 1; j++)
            {
                newActionsArray += actionsArray[j];
            }
            for (int j = 0; j < gesturesArray.Length - 1; j++)
            {
                newGesturesArray += gesturesArray[j];
            }
            SQLiteDataAccess.ChangeConnectors(MasterWindow.connectedUser.UserId, newGesturesArray, newActionsArray);
                
        }
    }
}
