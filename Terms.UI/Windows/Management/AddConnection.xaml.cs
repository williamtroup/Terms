using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Terms.Tools.Actions;
using Terms.Tools.Extensions;
using Terms.Tools.Settings.Interfaces;
using Terms.UI.Tools;
using Terms.UI.Tools.Actions;
using Terms.UI.Tools.Controls;
using Terms.UI.Tools.Extensions;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.ViewModels.Storage;
using Terms.UI.Tools.Views;
using Terms.Windows.Tools;

namespace Terms.Windows.Management;

public partial class AddConnection
{
    private const int MaximumPortAllowed = 65535;
    private const int MinimumWidthAllowed = 640;
    private const int MimimumHeightAllowed = 480;

    private readonly IXmlSettings m_settings;
    private readonly Main m_main;
    private readonly Credentials m_credentials;
    private readonly FilenameDialog m_filenameDialog;
    private readonly int m_selectedIndex;
    private readonly Connection m_connectionViewModel;

    public AddConnection(
        IXmlSettings settings,
        Main main,
        Credentials credentials,
        FilenameDialog filenameDialog,
        int selectedIndex = -1,
        Connection connection = null)
    {
        InitializeComponent();

        WindowLayout.Setup(this, WindowBorder, sideBorders: new List<Border> { bdSidePanel });

        m_settings = settings;
        m_main = main;
        m_credentials = credentials;
        m_filenameDialog = filenameDialog;
        m_selectedIndex = selectedIndex;
        m_connectionViewModel = connection;

        SetupDisplay();
    }

    private void SetupDisplay()
    {
        int closeWindowAfterAdding = Convert.ToInt32(m_settings.Read(Settings.AddNewConnectionWindow.AddNewConnectionOptions, nameof(Settings.AddNewConnectionWindow.CloseWindowAfterAdding), Settings.AddNewConnectionWindow.CloseWindowAfterAdding));

        chkCloseWindowAfterAdding.IsChecked = closeWindowAfterAdding > 0;

        lblErrorMessage.Visibility = Visibility.Hidden;

        Connection connection = new();

        if (m_selectedIndex > -1 || m_connectionViewModel != null)
        {
            chkCloseWindowAfterAdding.IsChecked = true;
            chkCloseWindowAfterAdding.Visibility = Visibility.Collapsed;

            txtName.Text = m_connectionViewModel.Name;
            txtAddress.Text = m_connectionViewModel.Address;
            txtPort.Text = m_connectionViewModel.Port.ToString();
            txtUsername.Text = m_connectionViewModel.Username;
            txtPassword.Password = Cypher.Decrypt(m_connectionViewModel.Password);
            chkAskForCredentailsBeforeConnecting.IsChecked = m_connectionViewModel.AskForCredentials;
            chkStartUsingLastPosition.IsChecked = m_connectionViewModel.StartUsingLastKnownPosition;
            chkStartInFullScreenMode.IsChecked = m_connectionViewModel.StartInFullScreenMode;
            chkSpanAcrossMultipleMonitors.IsChecked = m_connectionViewModel.SpanAcrossMultipleMonitors;
            chkLoginUsingAdminMode.IsChecked = m_connectionViewModel.LoginUsingAdminMode;
            chkUseSpecificWidthAndHeight.IsChecked = m_connectionViewModel.UseSpecificWidthAndHeight;
            txtWidth.Text = m_connectionViewModel.Width.ToString();
            txtHeight.Text = m_connectionViewModel.Height.ToString();
            txtNotes.Text = m_connectionViewModel.Notes;
            txtProgram.Text = m_connectionViewModel.BeforeOpeningProgram;
            txtArguments.Text = m_connectionViewModel.BeforeOpeningArguments;
            txtWorkingDirectory.Text = m_connectionViewModel.BeforeOpeningWorkingDirectory;
            chkWaitForProgramToBeClosed.IsChecked = m_connectionViewModel.BeforeOpeningWaitForProgramToBeClosed;
            chkAlwaysRunProgramBeforeConnecting.IsChecked = m_connectionViewModel.BeforeOpeningAlwaysRunProgramBeforeConnecting;
            txtWaitMillisecondsToBeClosed.Text = m_connectionViewModel.BeforeOpeningWaitSecondsForProgramToBeClosed.ToString();
            chkDeleteCachedCredentialsAfterConnecting.IsChecked = m_connectionViewModel.DeleteCachedCredentialsAfterConnecting;
            chkEnabled.IsChecked = m_connectionViewModel.Enabled;

            if (m_selectedIndex > -1)
            {
                bAdd.Content = Terms.Resources.UIMessages.EditButtonText;
            }
            else
            {
                if (!string.IsNullOrEmpty(txtName.Text))
                {
                    txtName.Text = string.Format(Terms.Resources.UIMessages.DuplicateNewName, txtName.Text);
                }
            }
        }
        else
        {
            txtWidth.Text = connection.Width.ToString();
            txtHeight.Text = connection.Height.ToString();
            txtPort.Text = connection.Port.ToString();
            txtWaitMillisecondsToBeClosed.Text = connection.BeforeOpeningWaitSecondsForProgramToBeClosed.ToString();
        }

        NumericInput.Create(txtWidth, connection.Width, MinimumWidthAllowed, validateValuesNow: true);
        NumericInput.Create(txtHeight, connection.Height, MimimumHeightAllowed, validateValuesNow: true);
        NumericInput.Create(txtPort, connection.Port, maximumValueAllowed: MaximumPortAllowed, validateValuesNow: true);
        NumericInput.Create(txtWaitMillisecondsToBeClosed, connection.BeforeOpeningWaitSecondsForProgramToBeClosed);

        txtName.Focus();
        txtName.SelectAll();

        rbCredentials.IsChecked = true;

        UpdateWindow();
    }

    private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (chkCloseWindowAfterAdding.IsVisible)
        {
            m_settings.Write(Settings.AddNewConnectionWindow.AddNewConnectionOptions, nameof(Settings.AddNewConnectionWindow.CloseWindowAfterAdding), chkCloseWindowAfterAdding.IsReallyChecked().ToNumericString());
        }
    }

    private void CheckBox_AskForCredentailsBeforeConnecting_CheckedChanged(object sender, RoutedEventArgs e)
    {
        UpdateWindow();
    }

    private void UpdateWindow()
    {
        txtUsername.IsEnabled = !chkAskForCredentailsBeforeConnecting.IsReallyChecked();
        txtPassword.IsEnabled = !chkAskForCredentailsBeforeConnecting.IsReallyChecked();
    }

    private void Button_GetHostNameFromAddress_OnClick(object sender, RoutedEventArgs e)
    {
        bGetHostNameFromAddress.IsEnabled = false;

        Thread thread = new(() =>
        {
            string newName = null;

            try
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry(txtAddress.Text);
                newName = ipHostEntry.HostName;
            }
            catch
            {
                // Ignore (will add logging later)
            }

            BackgroundAction.Run(() =>
            {
                if (!string.IsNullOrEmpty(newName))
                {
                    txtName.Text = newName;
                }

                bGetHostNameFromAddress.IsEnabled = true;
            });
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    private void Button_PingAddress_OnClick(object sender, RoutedEventArgs e)
    {
        string name = string.IsNullOrEmpty(txtName.Text) ? null : txtName.Text;

        PingConnection pingConnection = new(m_settings, m_filenameDialog, txtAddress.Text, name)
        {
            Topmost = Topmost,
            Owner = this
        };

        pingConnection.ShowDialog();
    }

    private void Button_GetProgram_OnClick(object sender, RoutedEventArgs e)
    {
        string filename = null;

        if (m_filenameDialog.Open(Terms.Resources.Dialog.ProgramsFilter, Terms.Resources.Dialog.Open, ref filename))
        {
            txtProgram.Text = filename;
        }
    }

    private void Button_Add_OnClick(object sender, RoutedEventArgs e)
    {
        string newName = txtName.Text.Trim();
        bool askForCredentailsBeforeConnecting = chkAskForCredentailsBeforeConnecting.IsReallyChecked();

        if (string.IsNullOrEmpty(newName))
        {
            SetErrorMessage(Terms.Resources.UIMessages.ANameHasNotBeenEntered);

            rbCredentials.IsChecked = true;

            txtName.Focus();
        }
        else if (m_main.DoesConnectionExist(newName, m_selectedIndex))
        {
            SetErrorMessage(Terms.Resources.UIMessages.ConnectionNameAlreadyExists);

            rbCredentials.IsChecked = true;

            txtName.Focus();
        }
        else if (string.IsNullOrEmpty(txtAddress.Text.Trim()))
        {
            SetErrorMessage(Terms.Resources.UIMessages.AnAddressHasNotBeenEntered);

            rbCredentials.IsChecked = true;

            txtAddress.Focus();
        }
        else if (string.IsNullOrEmpty(txtUsername.Text.Trim()) && !askForCredentailsBeforeConnecting)
        {
            SetErrorMessage(Terms.Resources.UIMessages.AUsernameHasNotBeenEntered);

            rbCredentials.IsChecked = true;

            txtUsername.Focus();
        }
        else if (string.IsNullOrEmpty(txtPassword.Password.Trim()) && !askForCredentailsBeforeConnecting)
        {
            SetErrorMessage(Terms.Resources.UIMessages.APasswordHasNotBeenEntered);

            rbCredentials.IsChecked = true;

            txtPassword.Focus();
        }
        else if (askForCredentailsBeforeConnecting && m_credentials.Count == 0)
        {
            SetErrorMessage(Terms.Resources.UIMessages.NoCredentialsHaveBeenSetup);

            rbCredentials.IsChecked = true;

            txtName.Focus();
        }
        else if (!string.IsNullOrEmpty(txtProgram.Text) && !File.Exists(txtProgram.Text))
        {
            SetErrorMessage(Terms.Resources.UIMessages.ProgramDoesNotExist);

            rbBeforeConnecting.IsChecked = true;

            txtProgram.Focus();
        }
        else if ((!string.IsNullOrEmpty(txtArguments.Text) || !string.IsNullOrEmpty(txtWorkingDirectory.Text)) && string.IsNullOrEmpty(txtProgram.Text))
        {
            SetErrorMessage(Terms.Resources.UIMessages.NoProgramHasBeenSpecified);

            rbBeforeConnecting.IsChecked = true;

            txtProgram.Focus();
        }
        else if (!string.IsNullOrEmpty(txtWorkingDirectory.Text) && !Directory.Exists(txtWorkingDirectory.Text))
        {
            SetErrorMessage(Terms.Resources.UIMessages.WorkingDirectoryDoesNotExist);

            rbBeforeConnecting.IsChecked = true;

            txtWorkingDirectory.Focus();
        }
        else
        {
            int port = Convert.ToInt32(txtPort.Text);
            int width = Convert.ToInt32(txtWidth.Text);
            int height = Convert.ToInt32(txtHeight.Text);
            int waitMillisecondsToBeClosed = Convert.ToInt32(txtWaitMillisecondsToBeClosed.Text);
            string lastAccessed = m_connectionViewModel != null && m_selectedIndex > -1 ? m_connectionViewModel.LastAccessed : Terms.Resources.UIMessages.Unknown;

            Connection connection = new()
            {
                Name = txtName.Text,
                Address = txtAddress.Text,
                Port = port,
                Username = txtUsername.Text,
                Password = Cypher.Encrypt(txtPassword.Password),
                AskForCredentials = askForCredentailsBeforeConnecting,
                StartUsingLastKnownPosition = chkStartUsingLastPosition.IsReallyChecked(),
                StartInFullScreenMode = chkStartInFullScreenMode.IsReallyChecked(),
                SpanAcrossMultipleMonitors = chkSpanAcrossMultipleMonitors.IsReallyChecked(),
                LoginUsingAdminMode = chkLoginUsingAdminMode.IsReallyChecked(),
                UseSpecificWidthAndHeight = chkUseSpecificWidthAndHeight.IsReallyChecked(),
                Width = width,
                Height = height,
                LastAccessed = lastAccessed,
                Notes = txtNotes.Text,
                BeforeOpeningProgram = txtProgram.Text,
                BeforeOpeningArguments = txtArguments.Text,
                BeforeOpeningWorkingDirectory = txtWorkingDirectory.Text,
                BeforeOpeningWaitForProgramToBeClosed = chkWaitForProgramToBeClosed.IsReallyChecked(),
                BeforeOpeningAlwaysRunProgramBeforeConnecting = chkAlwaysRunProgramBeforeConnecting.IsReallyChecked(),
                BeforeOpeningWaitSecondsForProgramToBeClosed = waitMillisecondsToBeClosed,
                DeleteCachedCredentialsAfterConnecting = chkDeleteCachedCredentialsAfterConnecting.IsReallyChecked(),
                Enabled = chkEnabled.IsReallyChecked()
            };

            m_main.AddConnection(connection, m_selectedIndex);

            if (chkCloseWindowAfterAdding.IsReallyChecked())
            {
                Close();
            }
            else
            {
                Connection blankConnectionViewModel = new();

                txtName.Text = blankConnectionViewModel.Name;
                txtAddress.Text = blankConnectionViewModel.Address;
                txtPort.Text = blankConnectionViewModel.Port.ToString();
                txtUsername.Text = blankConnectionViewModel.Username;
                txtPassword.Password = blankConnectionViewModel.Password;
                txtWidth.Text = blankConnectionViewModel.Width.ToString();
                txtHeight.Text = blankConnectionViewModel.Height.ToString();
                txtNotes.Text = blankConnectionViewModel.Notes;
                txtProgram.Text = blankConnectionViewModel.BeforeOpeningProgram;
                txtArguments.Text = blankConnectionViewModel.BeforeOpeningArguments;
                txtWorkingDirectory.Text = blankConnectionViewModel.BeforeOpeningWorkingDirectory;
                txtWaitMillisecondsToBeClosed.Text = blankConnectionViewModel.BeforeOpeningWaitSecondsForProgramToBeClosed.ToString();

                chkAskForCredentailsBeforeConnecting.IsChecked = blankConnectionViewModel.AskForCredentials;
                chkStartUsingLastPosition.IsChecked = blankConnectionViewModel.StartUsingLastKnownPosition;
                chkStartInFullScreenMode.IsChecked = blankConnectionViewModel.StartInFullScreenMode;
                chkSpanAcrossMultipleMonitors.IsChecked = blankConnectionViewModel.SpanAcrossMultipleMonitors;
                chkLoginUsingAdminMode.IsChecked = blankConnectionViewModel.LoginUsingAdminMode;
                chkUseSpecificWidthAndHeight.IsChecked = blankConnectionViewModel.UseSpecificWidthAndHeight;
                chkWaitForProgramToBeClosed.IsChecked = blankConnectionViewModel.BeforeOpeningWaitForProgramToBeClosed;
                chkAlwaysRunProgramBeforeConnecting.IsChecked = blankConnectionViewModel.BeforeOpeningAlwaysRunProgramBeforeConnecting;
                chkDeleteCachedCredentialsAfterConnecting.IsChecked = blankConnectionViewModel.DeleteCachedCredentialsAfterConnecting;
                chkEnabled.IsChecked = blankConnectionViewModel.Enabled;

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

    private void Tab_OnChecked(object sender, RoutedEventArgs e)
    {
        if (gCredentials != null)
        {
            gCredentials.Visibility = Visibility.Collapsed;
            gSettings.Visibility = Visibility.Collapsed;
            gNotes.Visibility = Visibility.Collapsed;
            gBeforeConnecting.Visibility = Visibility.Collapsed;

            if (rbCredentials.IsReallyChecked())
            {
                gCredentials.Visibility = Visibility.Visible;

                txtName.Focus();
            }
            else if (rbSettings.IsReallyChecked())
            {
                gSettings.Visibility = Visibility.Visible;
            }
            else if (rbNotes.IsReallyChecked())
            {
                gNotes.Visibility = Visibility.Visible;

                txtNotes.Focus();
            }
            else if (rbBeforeConnecting.IsReallyChecked())
            {
                gBeforeConnecting.Visibility = Visibility.Visible;

                txtProgram.Focus();
            }
        }
    }
}