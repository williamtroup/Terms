using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Terms.UI.Tools.Actions;
using Terms.UI.Tools.Shell;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.ViewModels.Storage;
using Terms.UI.Tools.Views;

namespace Terms.Windows.Management;

public partial class PickCredentials
{
    private readonly MstscProcesses m_mstscProcesses;
    private readonly Credentials m_credentials;
    private readonly List<Connection> m_connections;
    private readonly bool m_rememberTheLastPickedUserCredentialsForConnections;
    private readonly bool m_pickingModeOnly;

    public PickCredentials(Credentials credentials)
    {
        InitializeComponent();

        WindowLayout.Setup(this, WindowBorder);

        m_credentials = credentials;
        m_pickingModeOnly = true;

        SetupDisplay();
    }

    public PickCredentials(MstscProcesses mstscProcesses, Credentials credentials, List<Connection> connections, bool rememberTheLastPickedUserCredentialsForConnections)
    {
        InitializeComponent();

        WindowLayout.Setup(this, WindowBorder);

        m_mstscProcesses = mstscProcesses;
        m_credentials = credentials;
        m_connections = connections;
        m_rememberTheLastPickedUserCredentialsForConnections = rememberTheLastPickedUserCredentialsForConnections;

        SetupDisplay();
    }

    private void SetupDisplay()
    {
        string lastUserCredentialNameUsed = m_rememberTheLastPickedUserCredentialsForConnections
            ? LastUserCredentialNameUsed()
            : null;

        foreach (Credential credential in m_credentials.UserCredentials)
        {
            if (credential.Enabled)
            {
                ListBoxItem listBoxItem = new()
                {
                    Content = credential.Name,
                    ToolTip = credential.Notes
                };

                if (!string.IsNullOrEmpty(lastUserCredentialNameUsed) && string.Equals(lastUserCredentialNameUsed, credential.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    listBoxItem.IsSelected = true;
                }

                lstCredentials.Items.Add(listBoxItem);
            }
        }

        if (m_pickingModeOnly)
        {
            bEnterManually.Visibility = Visibility.Collapsed;
        }
        else
        {
            if (m_connections.Count > 1)
            {
                bEnterManually.IsEnabled = false;
            }
        }
    }

    private string LastUserCredentialNameUsed()
    {
        HashSet<string> names = new();

        foreach (Connection connection in m_connections)
        {
            if (!string.IsNullOrEmpty(connection.LastUserCredentialNameUsed))
            {
                names.Add(connection.LastUserCredentialNameUsed);
            }
        }

        return names.Count == 1 ? names.First() : null;
    }

    public Credential CredentailSelected { get; private set; }

    private void Credentials_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Button_Pick_Click(sender, null);
        }
    }

    private void Credentials_OnKeyDown(object sender, KeyEventArgs e)
    {
        ListAction.FindOnKeydown(e.Key.ToString(), lstCredentials);
    }

    private void Button_Manual_Click(object sender, RoutedEventArgs e)
    {
        LoadMstsc(true);
    }

    private void Button_Pick_Click(object sender, RoutedEventArgs e)
    {
        LoadMstsc();
    }

    private void LoadMstsc(bool enterManually = false)
    {
        if (lstCredentials.SelectedIndex > -1 || enterManually)
        {
            Credential credential = !enterManually ? m_credentials.UserCredentials[lstCredentials.SelectedIndex] : null;

            if (credential != null && (m_rememberTheLastPickedUserCredentialsForConnections || m_pickingModeOnly))
            {
                CredentailSelected = credential;
            }

            if (!m_pickingModeOnly)
            {
                List<Connection> newConnections = new();

                foreach (Connection connection in m_connections)
                {
                    Connection newConnectionViewModel = new();
                    newConnectionViewModel.Update(connection);

                    if (!enterManually && credential != null)
                    {
                        newConnectionViewModel.Username = credential.Username;
                        newConnectionViewModel.Password = credential.Password;
                    }

                    newConnections.Add(newConnectionViewModel);

                    if (enterManually)
                    {
                        Mstsc.DeleteCachedCredentials(newConnectionViewModel);
                    }
                }

                Mstsc.Open(m_mstscProcesses, newConnections, !enterManually);
            }

            DialogResult = true;

            Close();
        }
    }
}