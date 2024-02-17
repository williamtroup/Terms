using System;
using System.Windows;
using Terms.Tools.Actions;
using Terms.Tools.Extensions;
using Terms.Tools.Settings.Interfaces;
using Terms.UI.Tools;
using Terms.UI.Tools.Extensions;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.Views;

namespace Terms.Windows.Management
{
    public partial class AddCredential
    {
        #region Private Read-Only Variables

        private readonly IXmlSettings m_settings;
        private readonly EditCredentials m_credentials;
        private readonly int m_selectedIndex;
        private readonly Credential m_userCredentialViewModel;

        #endregion

        public AddCredential(IXmlSettings settings, EditCredentials credentials, int selectedIndex = -1, Credential credential = null)
        {
            InitializeComponent();

            WindowLayout.Setup(this, WindowBorder);

            m_settings = settings;
            m_credentials = credentials;
            m_selectedIndex = selectedIndex;
            m_userCredentialViewModel = credential;

            SetupDisplay();
        }

        private void SetupDisplay()
        {
            int closeWindowAfterAdding = Convert.ToInt32(m_settings.Read(Settings.AddNewUserCredentialWindow.AddNewUserCredentialOptions, nameof(Settings.AddNewUserCredentialWindow.CloseWindowAfterAdding), Settings.AddNewUserCredentialWindow.CloseWindowAfterAdding));

            chkCloseWindowAfterAdding.IsChecked = closeWindowAfterAdding > 0;

            lblErrorMessage.Visibility = Visibility.Hidden;

            rbCredentials.IsChecked = true;

            if (m_selectedIndex > -1)
            {
                chkCloseWindowAfterAdding.IsChecked = true;
                chkCloseWindowAfterAdding.Visibility = Visibility.Collapsed;

                bAdd.Content = Terms.Resources.UIMessages.EditButtonText;

                txtName.Text = m_userCredentialViewModel.Name;
                txtUsername.Text = m_userCredentialViewModel.Username;
                txtPassword.Password = Cypher.Decrypt(m_userCredentialViewModel.Password);
                txtNotes.Text = m_userCredentialViewModel.Notes;
                chkEnabled.IsChecked = m_userCredentialViewModel.Enabled;
            }

            txtName.Focus();
            txtName.SelectAll();
        }

        #region Private "Window" Events

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (chkCloseWindowAfterAdding.IsVisible)
            {
                m_settings.Write(Settings.AddNewUserCredentialWindow.AddNewUserCredentialOptions, nameof(Settings.AddNewUserCredentialWindow.CloseWindowAfterAdding), chkCloseWindowAfterAdding.IsReallyChecked().ToNumericString());
            }
        }

        #endregion

        #region Private "Add" Events

        private void Button_Add_OnClick(object sender, RoutedEventArgs e)
        {
            string newName = txtName.Text.Trim();

            if (string.IsNullOrEmpty(newName))
            {
                SetErrorMessage(Terms.Resources.UIMessages.ANameHasNotBeenEntered);

                rbCredentials.IsChecked = true;

                txtName.Focus();
            }
            else if (m_credentials.DoesUserCredentialExist(newName, m_selectedIndex))
            {
                SetErrorMessage(Terms.Resources.UIMessages.UserCredentialNameAlreadyExist);

                rbCredentials.IsChecked = true;

                txtName.Focus();
            }
            else if (string.IsNullOrEmpty(txtUsername.Text.Trim()))
            {
                SetErrorMessage(Terms.Resources.UIMessages.AUsernameHasNotBeenEntered);

                rbCredentials.IsChecked = true;

                txtUsername.Focus();
            }
            else if (string.IsNullOrEmpty(txtPassword.Password.Trim()))
            {
                SetErrorMessage(Terms.Resources.UIMessages.APasswordHasNotBeenEntered);

                rbCredentials.IsChecked = true;

                txtPassword.Focus();
            }
            else
            {
                m_credentials.AddUserCredential(new Credential
                {
                    Name = txtName.Text,
                    Username = txtUsername.Text,
                    Password = Cypher.Encrypt(txtPassword.Password),
                    Notes = txtNotes.Text,
                    Enabled = chkEnabled.IsReallyChecked()
                }, m_selectedIndex);

                if (chkCloseWindowAfterAdding.IsReallyChecked())
                {
                    Close();
                }
                else
                {
                    txtName.Text = string.Empty;
                    txtUsername.Text = string.Empty;
                    txtPassword.Password = string.Empty;
                    txtNotes.Text = string.Empty;

                    chkEnabled.IsChecked = true;

                    rbCredentials.IsChecked = true;

                    lblErrorMessage.Visibility = Visibility.Hidden;

                    txtName.Focus();
                }
            }
        }

        private void SetErrorMessage(string message)
        {
            lblErrorMessage.Content = message;
            lblErrorMessage.Visibility = Visibility.Visible;
        }

        #endregion

        #region Private "Tab Display" Events

        private void Tab_OnChecked(object sender, RoutedEventArgs e)
        {
            if (gCredentials != null)
            {
                gCredentials.Visibility = Visibility.Collapsed;
                gNotes.Visibility = Visibility.Collapsed;

                if (rbCredentials.IsReallyChecked())
                {
                    gCredentials.Visibility = Visibility.Visible;

                    txtName.Focus();
                }
                else if (rbNotes.IsReallyChecked())
                {
                    gNotes.Visibility = Visibility.Visible;

                    txtNotes.Focus();
                }
            }
        }

        #endregion
    }
}