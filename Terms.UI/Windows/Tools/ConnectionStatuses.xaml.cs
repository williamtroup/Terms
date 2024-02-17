using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Terms.Tools.Extensions;
using Terms.Tools.Settings.Interfaces;
using Terms.UI.Tools;
using Terms.UI.Tools.Actions;
using Terms.UI.Tools.Extensions;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.Views;
using Terms.Windows.Display;

namespace Terms.Windows.Tools
{
    public partial class ConnectionStatuses
    {
        #region Private Read-Only Variables

        private readonly IXmlSettings m_settings;
        private readonly List<Connection> m_connections;
        private readonly Main m_main;
        private readonly bool m_showRemoveUnavailableConnectionsButtons;
        private readonly ListViewSettings m_listViewSettings;

        #endregion

        #region Private Variables

        private bool m_updateWindowThreadRunning = true;
        private int m_totalAvailable;
        private int m_totalUnavailable;
        private List<string> m_unavailableConnectionsNames;

        #endregion

        public ConnectionStatuses(IXmlSettings settings, List<Connection> connections, Main main, bool showRemoveUnavailableConnectionsButtons = true)
        {
            InitializeComponent();

            WindowLayout.Setup(this, WindowBorder);

            m_settings = settings;
            m_connections = connections;
            m_main = main;
            m_showRemoveUnavailableConnectionsButtons = showRemoveUnavailableConnectionsButtons;

            XmlDocument xmlDocument = m_settings.GetDocument();

            m_listViewSettings = new ListViewSettings(m_settings, lstvConnectionStatuses, GetName, xmlDocument);

            SetupDisplay(xmlDocument);
        }

        private static string GetName => string.Format(Settings.WindowNameFormat, nameof(ConnectionStatuses), Settings.Window);

        private void SetupDisplay(XmlDocument xmlDocument)
        {
            int automaticallyScrollToTheBottom = Convert.ToInt32(m_settings.Read(Settings.ConnectionStatusesWindow.ConnectionStatusesOptions, nameof(Settings.ConnectionStatusesWindow.AutomaticallyScrollToTheBottom), Settings.ConnectionStatusesWindow.AutomaticallyScrollToTheBottom, xmlDocument));
            int onlyShowUnavailableConnections = Convert.ToInt32(m_settings.Read(Settings.ConnectionStatusesWindow.ConnectionStatusesOptions, nameof(Settings.ConnectionStatusesWindow.OnlyShowUnavailableConnections), Settings.ConnectionStatusesWindow.OnlyShowUnavailableConnections, xmlDocument));

            chkAutomaticallyScrollToTheBottom.IsChecked = automaticallyScrollToTheBottom > 0;
            chkOnlyShowUnavailableConnections.IsChecked = onlyShowUnavailableConnections > 0;

            bDeleteUnavailableConnections.Visibility = m_showRemoveUnavailableConnectionsButtons ? Visibility.Visible : Visibility.Hidden;

            SetupWindow();
            SetupWindowUpdateThread();
        }

        private void SetupWindow()
        {
            m_totalAvailable = 0;
            m_totalUnavailable = 0;
            m_unavailableConnectionsNames = new List<string>();

            bDeleteUnavailableConnections.IsEnabled = false;

            if (lstvConnectionStatuses.Items.Count > 0)
            {
                lstvConnectionStatuses.Items.Clear();
            }

            lblTitle.Text = string.Format(Terms.Resources.UIMessages.CheckingConnectionStatus, lstvConnectionStatuses.Items.Count + 1, m_connections.Count);
        }

        #region Private "Window" Events

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_updateWindowThreadRunning = false;

            XmlDocument xmlDocument = m_settings.GetDocument();

            m_settings.Write(Settings.ConnectionStatusesWindow.ConnectionStatusesOptions, nameof(Settings.ConnectionStatusesWindow.AutomaticallyScrollToTheBottom), chkAutomaticallyScrollToTheBottom.IsReallyChecked().ToNumericString(), xmlDocument);
            m_settings.Write(Settings.ConnectionStatusesWindow.ConnectionStatusesOptions, nameof(Settings.ConnectionStatusesWindow.OnlyShowUnavailableConnections), chkOnlyShowUnavailableConnections.IsReallyChecked().ToNumericString(), xmlDocument);

            m_listViewSettings.SetColumnWidths(xmlDocument);

            m_settings.SaveDocument(xmlDocument);
        }

        #endregion

        #region Private Pinging Helpers

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
                foreach (Connection connection in m_connections)
                {
                    Task.Run(() =>
                    {
                        if (m_updateWindowThreadRunning)
                        {
                            using (Ping ping = new())
                            {
                                PingReply pingReply = ping.ToResult(connection.Address);
                                string status = Terms.Resources.UIMessages.Unavailable;

                                if (pingReply != null && m_updateWindowThreadRunning)
                                {
                                    if (pingReply.Status == IPStatus.Success)
                                    {
                                        status = Terms.Resources.UIMessages.Available;
                                        m_totalAvailable++;
                                    }
                                    else
                                    {
                                        m_totalUnavailable++;
                                        m_unavailableConnectionsNames.Add(connection.Name);
                                    }
                                }

                                BackgroundAction.Run(() =>
                                {
                                    UpdateResults(connection.Name, connection.Address, status);
                                });
                            }
                        }
                    });

                    if (!m_updateWindowThreadRunning)
                    {
                        break;
                    }
                }

                break;
            }
        }

        private void UpdateResults(string name, string address, string status)
        {
            ConnectionStatus connectionStatus = new()
            {
                Name = name,
                Address = address,
                Status = status
            };

            if (chkOnlyShowUnavailableConnections.IsReallyChecked() && status == Terms.Resources.UIMessages.Available)
            {
                connectionStatus.Visibility = Visibility.Collapsed;
            }

            if (status == Terms.Resources.UIMessages.Unavailable)
            {
                connectionStatus.ForeColor = Brushes.Gray;
                connectionStatus.FontStyle = FontStyles.Italic;
            }

            lstvConnectionStatuses.Items.Add(connectionStatus);

            if (chkAutomaticallyScrollToTheBottom.IsReallyChecked())
            {
                lstvConnectionStatuses.ScrollIntoView(lstvConnectionStatuses.Items[lstvConnectionStatuses.Items.Count - 1]);
            }

            if (lstvConnectionStatuses.Items.Count < m_connections.Count)
            {
                lblTitle.Text = string.Format(Terms.Resources.UIMessages.CheckingConnectionStatus, lstvConnectionStatuses.Items.Count + 1, m_connections.Count);
            }
            else
            {
                ShowWindowAsCompleted();
            }
        }

        private void ShowWindowAsCompleted()
        {
            lblTitle.Text = string.Format(Terms.Resources.UIMessages.ConnectionStatuses, m_totalAvailable, m_connections.Count);

            bStop.Content = Terms.Resources.UIMessages.Restart;

            if (m_totalUnavailable > 0)
            {
                bDeleteUnavailableConnections.IsEnabled = true;
            }
        }

        #endregion

        #region Private "Button" Events

        private void Button_Stop_OnClick(object sender, RoutedEventArgs e)
        {
            if (m_updateWindowThreadRunning)
            {
                m_updateWindowThreadRunning = false;

                ShowWindowAsCompleted();
            }
            else
            {
                m_updateWindowThreadRunning = true;

                SetupWindow();
                SetupWindowUpdateThread();

                bStop.Content = Terms.Resources.UIMessages.Stop;
            }
        }

        private void Button_DeleteUnavailableConnections_OnClick(object sender, RoutedEventArgs e)
        {
            MessageQuestion messageBox = new(m_settings, Terms.Resources.UIMessages.DeleteUnavailableConnectionsConfirmation)
            {
                Topmost = Topmost,
                Owner = this
            };

            bool? result = messageBox.ShowDialog();

            if (result != null && result.Value)
            {
                m_main.RemovedConnectionsByName(m_unavailableConnectionsNames);

                Close();
            }
        }

        #endregion

        #region Private "CheckBox" Events

        private void CheckBox_OnlyShowUnavailableConnections_CheckedChanged(object sender, RoutedEventArgs e)
        {
            foreach (object statusItem in lstvConnectionStatuses.Items)
            {
                ConnectionStatus connectionStatus = (ConnectionStatus) statusItem;

                if (!chkOnlyShowUnavailableConnections.IsReallyChecked())
                {
                    connectionStatus.Visibility = Visibility.Visible;
                }
                else
                {
                    if (chkOnlyShowUnavailableConnections.IsReallyChecked() && connectionStatus.Status == Terms.Resources.UIMessages.Unavailable)
                    {
                        connectionStatus.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        connectionStatus.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        #endregion

        #region Private "ListView" Events

        private void ConnectionStatuses_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            ListViewOrdering listViewOrdering = new(lstvConnectionStatuses, e);
            listViewOrdering.Sort();
        }

        private void ListView_ConnectionStatuses_OnKeyDown(object sender, KeyEventArgs e)
        {
            ListViewAction.FindOnKeydown(e.Key.ToString(), lstvConnectionStatuses);
        }

        #endregion
    }
}