using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Terms.Tools.Settings.Interfaces;
using Terms.Tools.Windows;
using Terms.UI.Tools;
using Terms.UI.Tools.Controls;
using Terms.UI.Tools.Extensions;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.Views;
using Terms.Windows.Display;

namespace Terms.Windows.Tools;

public partial class ShutdownConnection
{
    private const int DefaultDelay = 0;

    private readonly List<Connection> m_connections;
    private readonly IXmlSettings m_settings;
    private readonly FilenameDialog m_filenameDialog;

    private bool m_wereConnectionsShutdown;

    public ShutdownConnection(List<Connection> connections, IXmlSettings settings, FilenameDialog filenameDialog)
    {
        InitializeComponent();

        m_connections = connections;
        m_settings = settings;
        m_filenameDialog = filenameDialog;

        WindowLayout.Setup(this, WindowBorder);

        SetupDisplay();
    }

    private void SetupDisplay()
    {
        lblErrorMessage.Visibility = Visibility.Hidden;

        string lastMessage = m_settings.Read(Settings.ShutdownConnectionsWindow.ShutdownConnectionOptions, nameof(Settings.ShutdownConnectionsWindow.LastMessage), Settings.ShutdownConnectionsWindow.LastMessage);
        string lastDelay = m_settings.Read(Settings.ShutdownConnectionsWindow.ShutdownConnectionOptions, nameof(Settings.ShutdownConnectionsWindow.LastDelay), Settings.ShutdownConnectionsWindow.LastDelay);

        NumericInput.Create(txtDelay, DefaultDelay);

        if (m_connections.Count > 1)
        {
            rdName.Height = new GridLength(0);
            rdAddress.Height = new GridLength(0);
            txtName.IsTabStop = false;
            txtAddress.IsTabStop = false;

            lblMessage.Margin = new Thickness(10, 10, 10, 0);
            txtMessage.Margin = new Thickness(0, 10, 10, 0);
        }
        else
        {
            txtName.Text = m_connections.First().Name;
            txtAddress.Text = m_connections.First().Address;
        }

        txtDelay.Text = lastDelay;

        txtMessage.Text = lastMessage;
        txtMessage.Focus();
        txtMessage.SelectAll();
    }

    private void Window_OnClosing(object sender, CancelEventArgs e)
    {
        if (m_wereConnectionsShutdown)
        {
            m_settings.Write(Settings.ShutdownConnectionsWindow.ShutdownConnectionOptions, nameof(Settings.ShutdownConnectionsWindow.LastMessage), txtMessage.Text);
            m_settings.Write(Settings.ShutdownConnectionsWindow.ShutdownConnectionOptions, nameof(Settings.ShutdownConnectionsWindow.LastDelay), txtDelay.Text);
        }
    }

    private void Button_PingAddress_OnClick(object sender, RoutedEventArgs e)
    {
        PingConnection pingConnection = new(m_settings, m_filenameDialog, txtAddress.Text, txtName.Text)
        {
            Topmost = Topmost,
            Owner = this
        };

        pingConnection.ShowDialog();
    }

    private void Button_Run_OnClick(object sender, RoutedEventArgs e)
    {
        if (uint.TryParse(txtDelay.Text, out uint delay) && GetConfirmationFromMessage(Terms.Resources.UIMessages.ShutdownRestartWarning))
        {
            foreach (Connection connection in m_connections)
            {
                if (!NativeMethods.InitiateSystemShutdownEx(
                        connection.Address,
                        txtMessage.Text,
                        delay,
                        true,
                        chkRestart.IsReallyChecked(),
                        ShutdownReason.SHTDN_REASON_MINOR_MAINTENANCE))
                {
                    SetErrorMessage(Terms.Resources.UIMessages.ShutdownError);
                }
            }

            m_wereConnectionsShutdown = true;

            DialogResult = true;

            Close();
        }
    }

    private void SetErrorMessage(string message)
    {
        lblErrorMessage.Content = message;
        lblErrorMessage.Visibility = Visibility.Visible;
    }

    private bool GetConfirmationFromMessage(string message)
    {
        MessageQuestion messageBox = new(m_settings, message)
        {
            Topmost = Topmost,
            Owner = this
        };

        bool? result = messageBox.ShowDialog();
        bool confirmed = result != null && result.Value;

        return confirmed;
    }
}