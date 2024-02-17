using System;
using System.Windows;
using Terms.Tools.Extensions;
using Terms.Tools.Settings.Interfaces;
using Terms.UI.Tools;
using Terms.UI.Tools.Extensions;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.Views;

namespace Terms.Windows.Management
{
    public partial class AddGroup
    {
        #region Private Read-Only Variables

        private readonly IXmlSettings m_settings;
        private readonly Main m_main;
        private readonly int m_selectedIndex;
        private readonly Group m_groupViewModel;

        #endregion

        public AddGroup(IXmlSettings settings, Main main, int selectedIndex = -1, Group group = null)
        {
            InitializeComponent();

            WindowLayout.Setup(this, WindowBorder);

            m_settings = settings;
            m_main = main;
            m_selectedIndex = selectedIndex;
            m_groupViewModel = group;

            SetupDisplay();
        }

        private void SetupDisplay()
        {
            int closeWindowAfterAdding = Convert.ToInt32(m_settings.Read(Settings.AddNewGroupWindow.AddNewGroupOptions, nameof(Settings.AddNewGroupWindow.CloseWindowAfterAdding), Settings.AddNewGroupWindow.CloseWindowAfterAdding));

            chkCloseWindowAfterAdding.IsChecked = closeWindowAfterAdding > 0;

            lblErrorMessage.Visibility = Visibility.Hidden;

            rbGroup.IsChecked = true;

            if (m_selectedIndex > -1)
            {
                chkCloseWindowAfterAdding.IsChecked = true;
                chkCloseWindowAfterAdding.Visibility = Visibility.Collapsed;

                bAdd.Content = Terms.Resources.UIMessages.EditButtonText;

                txtName.Text = m_groupViewModel.Name;
                txtNotes.Text = m_groupViewModel.Notes;
                chkAllowMultipleConnectionManagement.IsChecked = m_groupViewModel.AllowMultipleConnectionManagement;
                chkAllowAllPasswordsToBeChanged.IsChecked = m_groupViewModel.AllowAllPasswordsToBeChanged;
            }
            else
            {
                SetupDefaults();
            }

            txtName.Focus();
            txtName.SelectAll();
        }

        #region Private "Window" Events

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (chkCloseWindowAfterAdding.IsVisible)
            {
                m_settings.Write(Settings.AddNewGroupWindow.AddNewGroupOptions, nameof(Settings.AddNewGroupWindow.CloseWindowAfterAdding), chkCloseWindowAfterAdding.IsReallyChecked().ToNumericString());
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

                rbGroup.IsChecked = true;

                txtName.Focus();
            }
            else if (m_main.DoesGroupExist(newName, m_selectedIndex))
            {
                SetErrorMessage(Terms.Resources.UIMessages.GroupNameAlreadyExists);

                rbGroup.IsChecked = true;

                txtName.Focus();
            }
            else
            {
                m_main.AddGroup(new Group
                {
                    Name = txtName.Text,
                    Notes = txtNotes.Text,
                    AllowMultipleConnectionManagement = chkAllowMultipleConnectionManagement.IsReallyChecked(),
                    AllowAllPasswordsToBeChanged = chkAllowAllPasswordsToBeChanged.IsReallyChecked()
                }, m_selectedIndex);

                if (chkCloseWindowAfterAdding.IsReallyChecked())
                {
                    Close();
                }
                else
                {
                    SetupDefaults();

                    rbGroup.IsChecked = true;

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

        private void SetupDefaults()
        {
            Group blankGroupViewModel = new Group();

            txtName.Text = blankGroupViewModel.Name;
            txtNotes.Text = blankGroupViewModel.Notes;

            chkAllowMultipleConnectionManagement.IsChecked = blankGroupViewModel.AllowMultipleConnectionManagement;
            chkAllowAllPasswordsToBeChanged.IsChecked = blankGroupViewModel.AllowAllPasswordsToBeChanged;
        }

        #endregion

        #region Private "Tab Display" Events

        private void Tab_OnChecked(object sender, RoutedEventArgs e)
        {
            if (gGroup != null)
            {
                gGroup.Visibility = Visibility.Collapsed;
                gNotes.Visibility = Visibility.Collapsed;

                if (rbGroup.IsReallyChecked())
                {
                    gGroup.Visibility = Visibility.Visible;

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