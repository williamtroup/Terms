using System.Threading;
using System.Windows;
using System.Windows.Input;
using Terms.Tools.Settings.Interfaces;
using Terms.UI.Tools;
using Terms.UI.Tools.Actions;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.ViewModels.Storage;
using Terms.UI.Tools.Views;
using Terms.Windows.Display;

namespace Terms.Windows.Management
{
    public partial class EditCredentials
    {
        private readonly IXmlSettings m_settings;
        private readonly Credentials m_credentials;
        private readonly FilenameDialog m_filenameDialog;
        private readonly ListViewSettings m_listViewSettings;

        private bool m_updateWindowThreadRunning = true;

        public EditCredentials(IXmlSettings settings, Credentials credentials, FilenameDialog filenameDialog)
        {
            InitializeComponent();

            WindowLayout.Setup(this, WindowBorder, false);

            Opacity = 0.0;

            m_settings = settings;
            m_credentials = credentials;
            m_filenameDialog = filenameDialog;

            m_listViewSettings = new ListViewSettings(m_settings, lstvUserCredentials, GetName);

            SetupDisplay();

            SetupWindowUpdateThread();
        }

        private static string GetName => string.Format(Settings.WindowNameFormat, nameof(EditCredentials), Settings.Window);

        private void SetupDisplay()
        {
            foreach (Credential credential in m_credentials.UserCredentials)
            {
                lstvUserCredentials.Items.Add(credential);
            }
        }

        public bool DoesUserCredentialExist(string name, int ignoreIndex = -1)
        {
            return ListViewAction.Find(name, lstvUserCredentials, ignoreIndex);
        }

        public void AddUserCredential(Credential credential, int selectedIndex = -1)
        {
            if (selectedIndex > -1)
            {
                Credential actualUserCredentialViewModel = (Credential) lstvUserCredentials.SelectedItem;
                actualUserCredentialViewModel.Update(credential);

                m_credentials.UserCredentials[selectedIndex] = credential;
            }
            else
            {
                lstvUserCredentials.Items.Add(credential);

                m_credentials.UserCredentials.Add(credential);
            }
        }

        private void SetupWindowUpdateThread()
        {
            Thread thread = new(WindowUpdateThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void WindowUpdateThread()
        {
            while (m_updateWindowThreadRunning)
            {
                BackgroundAction.Run(() =>
                {
                    bool isOneItemSelected = lstvUserCredentials.SelectedIndex > -1;
                    bool isListPopulated = lstvUserCredentials.Items.Count > 0;
                    bool isTopItemSelected = lstvUserCredentials.SelectedIndex == 0;
                    bool isBottomItemSelected = lstvUserCredentials.SelectedIndex == lstvUserCredentials.Items.Count - 1;

                    bEdit.IsEnabled = isOneItemSelected;
                    bDelete.IsEnabled = isOneItemSelected;
                    bMoveUp.IsEnabled = isOneItemSelected && !isTopItemSelected;
                    bMoveDown.IsEnabled = isOneItemSelected && !isBottomItemSelected;
                    bExport.IsEnabled = isListPopulated;
                    bClear.IsEnabled = isListPopulated;

                    UpdateOpacityDisplay();
                });

                Thread.Sleep(50);
            }
        }

        private void UpdateOpacityDisplay()
        {
            if (Opacity <= 0)
            {
                Opacity = 1.0;
            }
        }

        private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Delete) && bDelete.IsEnabled)
            {
                Button_Delete_Click(sender, null);
            }
            else if (KeyStroke.IsAltKey(Key.Space))
            {
                e.Handled = true;
            }
        }

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_updateWindowThreadRunning = false;

            m_listViewSettings.SetColumnWidths();
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            AddCredential addCredential = new(m_settings, this)
            {
                Topmost = Topmost,
                Owner = this
            };

            addCredential.ShowDialog();
        }

        private void Button_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (lstvUserCredentials.SelectedIndex > -1)
            {
                Credential credential = (Credential) lstvUserCredentials.SelectedItem;

                AddCredential addCredential = new(m_settings, this, lstvUserCredentials.SelectedIndex, credential)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                addCredential.ShowDialog();
            }
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = lstvUserCredentials.SelectedIndex;
            if (selectedIndex > -1)
            {
                lstvUserCredentials.Items.RemoveAt(selectedIndex);

                m_credentials.UserCredentials.RemoveAt(selectedIndex);
            }
        }

        private void Button_MoveUp_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = lstvUserCredentials.SelectedIndex;
            if (selectedIndex > 0)
            {
                Credential credential = (Credential) lstvUserCredentials.Items[selectedIndex];

                lstvUserCredentials.Items.RemoveAt(selectedIndex);
                lstvUserCredentials.Items.Insert(selectedIndex - 1, credential);

                m_credentials.UserCredentials.RemoveAt(selectedIndex);
                m_credentials.UserCredentials.Insert(selectedIndex - 1, credential);

                lstvUserCredentials.SelectedIndex = selectedIndex - 1;
            }
        }

        private void Button_MoveDown_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = lstvUserCredentials.SelectedIndex;
            if (selectedIndex < lstvUserCredentials.Items.Count - 1)
            {
                Credential credential = (Credential) lstvUserCredentials.Items[selectedIndex];

                lstvUserCredentials.Items.RemoveAt(selectedIndex);
                lstvUserCredentials.Items.Insert(selectedIndex + 1, credential);

                m_credentials.UserCredentials.RemoveAt(selectedIndex);
                m_credentials.UserCredentials.Insert(selectedIndex + 1, credential);

                lstvUserCredentials.SelectedIndex = selectedIndex + 1;
            }
        }

        private void Button_Import_Click(object sender, RoutedEventArgs e)
        {
            string filename = null;

            if (m_filenameDialog.Open(Terms.Resources.Dialog.SupportedFileFilter, Terms.Resources.Dialog.OpenUserCredentialsFile, ref filename))
            {
                Credentials credentials = new();
                credentials.Load(filename);

                foreach (Credential credential in credentials.UserCredentials)
                {
                    if (!DoesUserCredentialExist(credential.Name))
                    {
                        lstvUserCredentials.Items.Add(credential);
                        m_credentials.UserCredentials.Add(credential);
                    }
                }
            }
        }

        private void Button_Export_Click(object sender, RoutedEventArgs e)
        {
            string filename = null;

            if (m_filenameDialog.Save(Terms.Resources.Dialog.SupportedFileFilter, Terms.Resources.Dialog.SaveUserCredentialsAs, ref filename))
            {
                m_credentials.Save(filename);
            }
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            MessageQuestion messageBox = new(m_settings, Terms.Resources.UIMessages.ClearUserCredentialsConfirmation)
            {
                Topmost = Topmost,
                Owner = this
            };

            bool? result = messageBox.ShowDialog();

            if (result != null && result.Value)
            {
                m_credentials.UserCredentials.Clear();

                lstvUserCredentials.Items.Clear();
            }
        }

        private void Credentials_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Button_Edit_Click(sender, null);
            }
        }

        private void Credentials_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            ListViewOrdering listViewOrdering = new(lstvUserCredentials, e);
            listViewOrdering.Sort();
        }

        private void Credentials_OnKeyDown(object sender, KeyEventArgs e)
        {
            ListViewAction.FindOnKeydown(e.Key.ToString(), lstvUserCredentials);
        }
    }
}