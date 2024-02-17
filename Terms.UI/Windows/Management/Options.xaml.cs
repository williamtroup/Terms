using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Microsoft.Win32;
using Terms.Tools.Extensions;
using Terms.Tools.Settings.Interfaces;
using Terms.UI.Tools;
using Terms.UI.Tools.Controls;
using Terms.UI.Tools.Extensions;
using Terms.UI.Tools.Shell;
using Terms.UI.Tools.Views;

namespace Terms.Windows.Management;

public partial class Options
{
    private const string AssemblyProductName = "Terms";
    private const int MinimumPingSettingAllowed = 500;
    private const int MaximumPingSettingAllowed = 60000;

    private bool m_clearLastAccessHistoryForAllConnections;
    private bool m_clearPortsForAllConnections;

    private readonly IXmlSettings m_settings;
    private readonly Main m_main;
    private readonly RegistryKey m_startUpRegistryKey;

    public Options(IXmlSettings settings, Main main)
    {
        InitializeComponent();

        m_settings = settings;
        m_main = main;
        m_startUpRegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        WindowLayout.Setup(this, WindowBorder, sideBorders: new List<Border> { bdSidePanel });

        SetupDisplay();
    }

    private void SetupDisplay()
    {
        XmlDocument xmlDocument = m_settings.GetDocument();

        lblWarningMessage.Visibility = Visibility.Hidden;

        string productStartUp = m_startUpRegistryKey.GetValue(AssemblyProductName, "").ToString();

        int showInTaskBar = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowInTaskBar), Settings.MainWindow.ShowInTaskBar, xmlDocument));
        int showConfirmationMessagesForFileActions = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowConfirmationMessagesForFileActions), Settings.MainWindow.ShowConfirmationMessagesForFileActions, xmlDocument));
        int minimizeMainWindowWhenAConnectionIsOpened = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.MinimizeMainWindowWhenAConnectionIsOpened), Settings.MainWindow.MinimizeMainWindowWhenAConnectionIsOpened, xmlDocument));
        int showOpenNewConnectionArea = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowOpenNewConnectionArea), Settings.MainWindow.ShowOpenNewConnectionArea, xmlDocument));
        int allowUserCredentialsPickingWhenOpeningConnections = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.AllowUserCredentialsPickingWhenOpeningConnections), Settings.MainWindow.AllowUserCredentialsPickingWhenOpeningConnections, xmlDocument));
        int rememberTheLastPickedUserCredentialsForConnections = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.RememberTheLastPickedUserCredentialsForConnections), Settings.MainWindow.RememberTheLastPickedUserCredentialsForConnections, xmlDocument));
        int showConnectionManagementButtonsInTheTitleBar = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowConnectionManagementButtonsInTheTitleBar), Settings.MainWindow.ShowConnectionManagementButtonsInTheTitleBar, xmlDocument));
        int showConnectionsOpenInTitleBarWhenAvailable = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowConnectionsOpenInTitleBarWhenAvailable), Settings.MainWindow.ShowConnectionsOpenInTitleBarWhenAvailable, xmlDocument));

        int showTheRunningConnectionsSection = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Connections, nameof(Settings.MainWindow.ShowTheRunningConnectionsSection), Settings.MainWindow.ShowTheRunningConnectionsSection, xmlDocument));
        int showConfirmationMessageBeforeClosingMainWindow = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Connections, nameof(Settings.MainWindow.ShowConfirmationMessageBeforeClosingMainWindow), Settings.MainWindow.ShowConfirmationMessageBeforeClosingMainWindow, xmlDocument));
        int minimizeAllOtherConnectionsWhenFocusingConnection = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Connections, nameof(Settings.MainWindow.MinimizeAllOtherConnectionsWhenFocusingConnection), Settings.MainWindow.MinimizeAllOtherConnectionsWhenFocusingConnection, xmlDocument));
        int updateLastAccessedForPingedConnections = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Connections, nameof(Settings.MainWindow.UpdateLastAccessedForPingedConnections), Settings.MainWindow.UpdateLastAccessedForPingedConnections, xmlDocument));
        int updateLastAccessedWhenConnectionsAreShutdownOrRestarted = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Connections, nameof(Settings.MainWindow.UpdateLastAccessedWhenConnectionsAreShutdownOrRestarted), Settings.MainWindow.UpdateLastAccessedWhenConnectionsAreShutdownOrRestarted, xmlDocument));

        int showLoadingSplashScreen = Convert.ToInt32(m_settings.Read(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.ShowLoadingSplashScreen), Settings.MainWindow.ShowLoadingSplashScreen, xmlDocument));
        int allowMultipleInstancesToBeUsed = Convert.ToInt32(m_settings.Read(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.AllowMultipleInstancesToBeUsed), Settings.MainWindow.AllowMultipleInstancesToBeUsed, xmlDocument));
        int checkToSeeIfNewUpdatesAreAvailable = Convert.ToInt32(m_settings.Read(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.CheckToSeeIfNewUpdatesAreAvailable), Settings.MainWindow.CheckToSeeIfNewUpdatesAreAvailable, xmlDocument));
        int fadeMainWindowInOutOnStartupShutdown = Convert.ToInt32(m_settings.Read(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.FadeMainWindowInOutOnStartupShutdown), Settings.MainWindow.FadeMainWindowInOutOnStartupShutdown, xmlDocument));

        int totalTimeBetweenEachPing = Convert.ToInt32(m_settings.Read(Settings.PingWindow.PingOptions, nameof(Settings.PingWindow.TotalTimeBetweenEachPing), Settings.PingWindow.TotalTimeBetweenEachPing.ToString(), xmlDocument));
        int totalTimeBetweenEachFailedPing = Convert.ToInt32(m_settings.Read(Settings.PingWindow.PingOptions, nameof(Settings.PingWindow.TotalTimeBetweenEachFailedPing), Settings.PingWindow.TotalTimeBetweenEachFailedPing.ToString(), xmlDocument));

        chkShowInTaskBar.IsChecked = showInTaskBar > 0;
        chkShowConfirmationMessagesForFileActions.IsChecked = showConfirmationMessagesForFileActions > 0;
        chkMinimizeMainWindowWhenAConnectionIsOpened.IsChecked = minimizeMainWindowWhenAConnectionIsOpened > 0;
        chkShowOpenNewConnectionArea.IsChecked = showOpenNewConnectionArea > 0;
        chkAllowUserCredentialsPickingWhenOpeningConnections.IsChecked = allowUserCredentialsPickingWhenOpeningConnections > 0;
        chkRememberTheLastPickedUserCredentialsForConnections.IsChecked = rememberTheLastPickedUserCredentialsForConnections > 0;
        chkShowConnectionManagementButtonsInTheTitleBar.IsChecked = showConnectionManagementButtonsInTheTitleBar > 0;
        chkShowConnectionsOpenInTitleBarWhenAvailable.IsChecked = showConnectionsOpenInTitleBarWhenAvailable > 0;

        chkShowTheRunningConnectionsSection.IsChecked = showTheRunningConnectionsSection > 0;
        chkShowConfirmationMessageBeforeClosingMainWindow.IsChecked = showConfirmationMessageBeforeClosingMainWindow > 0;
        chkMinimizeAllOtherConnectionsWhenFocusingConnection.IsChecked = minimizeAllOtherConnectionsWhenFocusingConnection > 0;
        chkUpdateLastAccessedForPingedConnections.IsChecked = updateLastAccessedForPingedConnections > 0;
        chkUpdateLastAccessedWhenConnectionsAreShutdownOrRestarted.IsChecked = updateLastAccessedWhenConnectionsAreShutdownOrRestarted > 0;

        chkOpenOnWindowsStartUp.IsChecked = !string.IsNullOrEmpty(productStartUp);
        chkShowLoadingSplashScreen.IsChecked = showLoadingSplashScreen > 0;
        chkAllowMultipleInstancesToBeUsed.IsChecked = allowMultipleInstancesToBeUsed > 0;
        chkCheckToSeeIfNewUpdatesAreAvailable.IsChecked = checkToSeeIfNewUpdatesAreAvailable > 0;
        chkFadeMainWindowInOutOnStartupShutdown.IsChecked = fadeMainWindowInOutOnStartupShutdown > 0;

        txtTotalTimeBetweenEachPing.Text = totalTimeBetweenEachPing.ToString();
        txtTotalTimeBetweenEachFailedPing.Text = totalTimeBetweenEachFailedPing.ToString();

        if (!MstscProcesses.IsEnvironmentValidForTracking)
        {
            chkShowTheRunningConnectionsSection.IsEnabled = false;
            chkShowConfirmationMessageBeforeClosingMainWindow.IsEnabled = false;
        }

        NumericInput.Create(txtTotalTimeBetweenEachPing, Settings.PingWindow.TotalTimeBetweenEachPing, MinimumPingSettingAllowed, MaximumPingSettingAllowed, true);
        NumericInput.Create(txtTotalTimeBetweenEachFailedPing, Settings.PingWindow.TotalTimeBetweenEachFailedPing, MinimumPingSettingAllowed, MaximumPingSettingAllowed, true);

        rbDisplay.IsChecked = true;

        UpdateTabDisplay();
    }

    private void Button_Apply_Click(object sender, RoutedEventArgs e)
    {
        XmlDocument xmlDocument = m_settings.GetDocument();

        SaveOpenOnWindowsStartUpSetting();

        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowInTaskBar), IsChecked(chkShowInTaskBar), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowConfirmationMessagesForFileActions), IsChecked(chkShowConfirmationMessagesForFileActions), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.MinimizeMainWindowWhenAConnectionIsOpened), IsChecked(chkMinimizeMainWindowWhenAConnectionIsOpened), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowOpenNewConnectionArea), IsChecked(chkShowOpenNewConnectionArea), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.AllowUserCredentialsPickingWhenOpeningConnections), IsChecked(chkAllowUserCredentialsPickingWhenOpeningConnections), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.RememberTheLastPickedUserCredentialsForConnections), IsChecked(chkRememberTheLastPickedUserCredentialsForConnections), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowConnectionManagementButtonsInTheTitleBar), IsChecked(chkShowConnectionManagementButtonsInTheTitleBar), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowConnectionsOpenInTitleBarWhenAvailable), IsChecked(chkShowConnectionsOpenInTitleBarWhenAvailable), xmlDocument);

        m_settings.Write(Settings.MainWindow.Connections, nameof(Settings.MainWindow.ShowTheRunningConnectionsSection), IsChecked(chkShowTheRunningConnectionsSection), xmlDocument);
        m_settings.Write(Settings.MainWindow.Connections, nameof(Settings.MainWindow.ShowConfirmationMessageBeforeClosingMainWindow), IsChecked(chkShowConfirmationMessageBeforeClosingMainWindow), xmlDocument);
        m_settings.Write(Settings.MainWindow.Connections, nameof(Settings.MainWindow.MinimizeAllOtherConnectionsWhenFocusingConnection), IsChecked(chkMinimizeAllOtherConnectionsWhenFocusingConnection), xmlDocument);
        m_settings.Write(Settings.MainWindow.Connections, nameof(Settings.MainWindow.UpdateLastAccessedForPingedConnections), IsChecked(chkUpdateLastAccessedForPingedConnections), xmlDocument);
        m_settings.Write(Settings.MainWindow.Connections, nameof(Settings.MainWindow.UpdateLastAccessedWhenConnectionsAreShutdownOrRestarted), IsChecked(chkUpdateLastAccessedWhenConnectionsAreShutdownOrRestarted), xmlDocument);

        m_settings.Write(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.ShowLoadingSplashScreen), IsChecked(chkShowLoadingSplashScreen), xmlDocument);
        m_settings.Write(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.AllowMultipleInstancesToBeUsed), IsChecked(chkAllowMultipleInstancesToBeUsed), xmlDocument);
        m_settings.Write(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.CheckToSeeIfNewUpdatesAreAvailable), IsChecked(chkCheckToSeeIfNewUpdatesAreAvailable), xmlDocument);
        m_settings.Write(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.FadeMainWindowInOutOnStartupShutdown), IsChecked(chkFadeMainWindowInOutOnStartupShutdown), xmlDocument);

        m_settings.Write(Settings.PingWindow.PingOptions, nameof(Settings.PingWindow.TotalTimeBetweenEachPing), txtTotalTimeBetweenEachPing.Text, xmlDocument);
        m_settings.Write(Settings.PingWindow.PingOptions, nameof(Settings.PingWindow.TotalTimeBetweenEachFailedPing), txtTotalTimeBetweenEachFailedPing.Text, xmlDocument);

        m_settings.SaveDocument(xmlDocument);

        m_main.InitializeDisplaySettings(xmlDocument);

        if (m_clearLastAccessHistoryForAllConnections)
        {
            m_main.ClearAllLastAccessedHistory();
        }

        if (m_clearPortsForAllConnections)
        {
            m_main.ClearAllConnectionPorts();
        }

        Close();
    }

    private void Button_ClearConnectionLastAccessedHistory_Click(object sender, RoutedEventArgs e)
    {
        m_clearLastAccessHistoryForAllConnections = true;

        bClearConnectionLastAccessedHistory.IsEnabled = false;

        lblWarningMessage.Visibility = Visibility.Visible;
    }

    private void Button_ClearConnectionPorts_Click(object sender, RoutedEventArgs e)
    {
        m_clearPortsForAllConnections = true;

        bClearConnectionPorts.IsEnabled = false;

        lblWarningMessage.Visibility = Visibility.Visible;
    }

    private void SaveOpenOnWindowsStartUpSetting()
    {
        string assemblyLocation = Process.GetCurrentProcess().MainModule.FileName;

        if (chkOpenOnWindowsStartUp.IsReallyChecked())
        {
            m_startUpRegistryKey.SetValue(AssemblyProductName, $"\"{assemblyLocation}\"");
        }
        else
        {
            m_startUpRegistryKey.DeleteValue(AssemblyProductName, false);
        }
    }

    private static string IsChecked(CheckBox checkbox)
    {
        return checkbox.IsReallyChecked().ToNumericString();
    }

    private void Tab_OnChecked(object sender, RoutedEventArgs e)
    {
        UpdateTabDisplay();
    }

    private void UpdateTabDisplay()
    {
        if (gDisplay != null)
        {
            gDisplay.Visibility = Visibility.Collapsed;
            gConnections.Visibility = Visibility.Collapsed;
            gPinging.Visibility = Visibility.Collapsed;
            gStartup.Visibility = Visibility.Collapsed;
            GClear.Visibility = Visibility.Collapsed;

            if (rbDisplay.IsReallyChecked())
            {
                gDisplay.Visibility = Visibility.Visible;
            }
            else if (rbConnections.IsReallyChecked())
            {
                gConnections.Visibility = Visibility.Visible;
            }
            else if (rbPinging.IsReallyChecked())
            {
                gPinging.Visibility = Visibility.Visible;

                txtTotalTimeBetweenEachPing.Focus();
            }
            else if (rbStartup.IsReallyChecked())
            {
                gStartup.Visibility = Visibility.Visible;
            }
            else if (rbClear.IsReallyChecked())
            {
                GClear.Visibility = Visibility.Visible;
            }
        }
    }
}