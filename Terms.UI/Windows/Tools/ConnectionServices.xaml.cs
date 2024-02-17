using System;
using System.Collections.Generic;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Terms.Tools.Actions;
using Terms.Tools.Extensions;
using Terms.Tools.Settings.Interfaces;
using Terms.UI.Tools;
using Terms.UI.Tools.Actions;
using Terms.UI.Tools.Enums;
using Terms.UI.Tools.Extensions;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.Views;

namespace Terms.Windows.Tools
{
    public partial class ConnectionServices
    {
        #region Private Read-Only Variables

        private readonly IXmlSettings m_settings;
        private readonly List<Connection> m_connections;
        private readonly Credential m_credential;
        private readonly ListViewSettings m_listViewSettings;

        #endregion

        #region Private Variables

        private bool m_updateWindowThreadRunning = true;
        private bool m_hasServiceBeenHandled;
        private ServicesMode m_servicesMode;
        private string m_serviceName;

        #endregion

        public ConnectionServices(IXmlSettings settings, List<Connection> connections, Credential credential)
        {
            InitializeComponent();

            WindowLayout.Setup(this, WindowBorder);

            m_settings = settings;
            m_connections = connections;
            m_credential = credential;

            XmlDocument xmlDocument = m_settings.GetDocument();

            m_listViewSettings = new ListViewSettings(m_settings, lstvConnectionServiceStatuses, GetName, xmlDocument);

            SetupDisplay(xmlDocument);
        }

        private static string GetName => string.Format(Settings.WindowNameFormat, nameof(ConnectionServices), Settings.Window);

        private void SetupDisplay(XmlDocument xmlDocument)
        {
            string lastUsedServiceName = m_settings.Read(Settings.ConnectionServicesWindow.ConnectionServicesOptions, nameof(Settings.ConnectionServicesWindow.LastUsedServiceName), Settings.ConnectionServicesWindow.LastUsedServiceName, xmlDocument);
            int automaticallyScrollToTheBottom = Convert.ToInt32(m_settings.Read(Settings.ConnectionServicesWindow.ConnectionServicesOptions, nameof(Settings.ConnectionServicesWindow.AutomaticallyScrollToTheBottom), Settings.ConnectionServicesWindow.AutomaticallyScrollToTheBottom, xmlDocument));
            int onlyShowTheUnsuccessfulServiceStatuses = Convert.ToInt32(m_settings.Read(Settings.ConnectionServicesWindow.ConnectionServicesOptions, nameof(Settings.ConnectionServicesWindow.OnlyShowTheUnsuccessfulServiceStatuses), Settings.ConnectionServicesWindow.OnlyShowTheUnsuccessfulServiceStatuses, xmlDocument));

            chkAutomaticallyScrollToTheBottom.IsChecked = automaticallyScrollToTheBottom > 0;
            chkOnlyShowTheUnsuccessfulServiceStatuses.IsChecked = onlyShowTheUnsuccessfulServiceStatuses > 0;

            txtServiceName.Text = lastUsedServiceName;
            txtServiceName.Focus();
            txtServiceName.SelectAll();

            SetupWindow();
        }

        private void SetupWindow()
        {
            if (lstvConnectionServiceStatuses.Items.Count > 0)
            {
                lstvConnectionServiceStatuses.Items.Clear();
            }
        }

        #region Private "Window" Events

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_updateWindowThreadRunning = false;

            XmlDocument xmlDocument = m_settings.GetDocument();

            if (m_hasServiceBeenHandled)
            {
                m_settings.Write(Settings.ConnectionServicesWindow.ConnectionServicesOptions, nameof(Settings.ConnectionServicesWindow.LastUsedServiceName), txtServiceName.Text, xmlDocument);
                m_settings.Write(Settings.ConnectionServicesWindow.ConnectionServicesOptions, nameof(Settings.ConnectionServicesWindow.AutomaticallyScrollToTheBottom), chkAutomaticallyScrollToTheBottom.IsReallyChecked().ToNumericString(), xmlDocument);
                m_settings.Write(Settings.ConnectionServicesWindow.ConnectionServicesOptions, nameof(Settings.ConnectionServicesWindow.OnlyShowTheUnsuccessfulServiceStatuses), chkOnlyShowTheUnsuccessfulServiceStatuses.IsReallyChecked().ToNumericString(), xmlDocument);
            }

            m_listViewSettings.SetColumnWidths(xmlDocument);

            m_settings.SaveDocument(xmlDocument);
        }

        #endregion

        #region Private "Running" Helpers

        private void SetupWindowUpdateThread()
        {
            Thread thread = new Thread(WindowUpdateThread);
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
                            try
                            {
                                ManagementScope managementScope = new ManagementScope($"\\\\{connection.Address}\\root\\cimv2", GetConnectionOptions());
                                managementScope.Connect();

                                HandleServices(managementScope, connection);
                            }
                            catch
                            {
                                AddServiceStatus(connection, Terms.Resources.UIMessages.ErrorConnectingToHost, false);
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

        private ConnectionOptions GetConnectionOptions()
        {
            return new ConnectionOptions
            {
                Impersonation = ImpersonationLevel.Impersonate,
                Authentication = AuthenticationLevel.Default,
                EnablePrivileges = true,
                Username = m_credential.Username,
                Password = Cypher.Decrypt(m_credential.Password)
            };
        }

        private void HandleServices(ManagementScope managementScope, Connection connection)
        {
            using (ManagementObjectCollection managementObjectCollection = GetObjectCollection(managementScope, $"SELECT State FROM Win32_Service WHERE Name = '{m_serviceName}'"))
            {
                bool handled = false;

                foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
                {
                    if (managementBaseObject is ManagementObject managementObject)
                    {
                        string state = managementObject["State"].ToString();
                        string status = null;
                        bool completed = false;

                        if (m_servicesMode == ServicesMode.Start && state != Terms.Resources.UIMessages.Running)
                        {
                            if (HandleService(managementScope, true))
                            {
                                status = Terms.Resources.UIMessages.Started;
                                completed = true;
                            }
                        }
                        else if (m_servicesMode == ServicesMode.Stop && state == Terms.Resources.UIMessages.Running)
                        {
                            if (HandleService(managementScope, false))
                            {
                                status = Terms.Resources.UIMessages.Stopped;
                                completed = true;
                            }
                        }
                        else if (m_servicesMode == ServicesMode.Status)
                        {
                            status = state;
                            completed = true;
                        }
                        else
                        {
                            status = Terms.Resources.UIMessages.Ignored;
                        }

                        if (string.IsNullOrEmpty(status))
                        {
                            status = Terms.Resources.UIMessages.CouldNotHandleService;
                        }

                        handled = true;

                        AddServiceStatus(connection, status, completed);
                    }
                }

                if (!handled)
                {
                    AddServiceStatus(connection, Terms.Resources.UIMessages.CannotFindService, false);
                }
            }
        }

        private void AddServiceStatus(Connection connection, string status, bool completed)
        {
            BackgroundAction.Run(() =>
            {
                ConnectionService connectionService = new ConnectionService
                {
                    Name = connection.Name,
                    Address = connection.Address,
                    Status = status,
                    Completed = completed
                };

                if (!completed)
                {
                    connectionService.ForeColor = Brushes.Gray;
                    connectionService.FontStyle = FontStyles.Italic;
                }

                if (chkOnlyShowTheUnsuccessfulServiceStatuses.IsReallyChecked() && completed)
                {
                    connectionService.Visibility = Visibility.Collapsed;
                }

                lstvConnectionServiceStatuses.Items.Add(connectionService);

                if (chkAutomaticallyScrollToTheBottom.IsReallyChecked())
                {
                    lstvConnectionServiceStatuses.ScrollIntoView(lstvConnectionServiceStatuses.Items[lstvConnectionServiceStatuses.Items.Count - 1]);
                }

                if (lstvConnectionServiceStatuses.Items.Count == m_connections.Count)
                {
                    Finished();
                }
            });
        }

        private static ManagementObjectCollection GetObjectCollection(ManagementScope managementScope, string query)
        {
            ObjectQuery objectQuery = new ObjectQuery(query);
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(managementScope, objectQuery);

            return managementObjectSearcher.Get();
        }

        private bool HandleService(ManagementScope managementScope, bool start)
        {
            bool handled = true;

            string command = start ? "StartService" : "StopService";
            string path = $"Win32_Service.Name='{m_serviceName}'";

            using (ManagementObject managementObject = new ManagementObject(managementScope, new ManagementPath(path), null))
            {
                try
                {
                    managementObject.InvokeMethod(command, null, null);
                }
                catch
                {
                    handled = false;
                }
            }

            return handled;
        }

        #endregion

        #region Private "ListView" Events

        private void ConnectionServiceStatuses_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            ListViewOrdering listViewOrdering = new ListViewOrdering(lstvConnectionServiceStatuses, e);
            listViewOrdering.Sort();
        }

        private void ListView_ConnectionServiceStatuses_OnKeyDown(object sender, KeyEventArgs e)
        {
            ListViewAction.FindOnKeydown(e.Key.ToString(), lstvConnectionServiceStatuses);
        }

        #endregion

        #region Private "Button" Events

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            Start(ServicesMode.Start);
        }

        private void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            Start(ServicesMode.Stop);
        }

        private void Button_Status_Click(object sender, RoutedEventArgs e)
        {
            Start(ServicesMode.Status);
        }

        private void Start(ServicesMode servicesMode)
        {
            m_servicesMode = servicesMode;

            m_updateWindowThreadRunning = true;
            m_hasServiceBeenHandled = true;

            m_serviceName = txtServiceName.Text;

            txtServiceName.IsEnabled = false;
            bStart.IsEnabled = false;
            bStop.IsEnabled = false;
            bStatus.IsEnabled = false;

            SetupWindow();
            SetupWindowUpdateThread();
        }

        private void Finished()
        {
            txtServiceName.IsEnabled = true;
            bStart.IsEnabled = true;
            bStop.IsEnabled = true;
            bStatus.IsEnabled = true;
        }

        #endregion

        #region Private "CheckBox" Events

        private void CheckBox_OnlyShowTheUnsuccessfulServiceStatuses_CheckedChanged(object sender, RoutedEventArgs e)
        {
            foreach (object serviceStatusItem in lstvConnectionServiceStatuses.Items)
            {
                ConnectionService connectionService = (ConnectionService) serviceStatusItem;

                if (!chkOnlyShowTheUnsuccessfulServiceStatuses.IsReallyChecked())
                {
                    connectionService.Visibility = Visibility.Visible;
                }
                else
                {
                    if (chkOnlyShowTheUnsuccessfulServiceStatuses.IsReallyChecked() && !connectionService.Completed)
                    {
                        connectionService.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        connectionService.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        #endregion
    }
}