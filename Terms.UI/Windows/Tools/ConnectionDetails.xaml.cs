using System;
using System.Collections.Generic;
using System.Management;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Terms.Resources;
using Terms.Tools.Actions;
using Terms.Tools.Extensions;
using Terms.Tools.Settings.Interfaces;
using Terms.UI.Tools;
using Terms.UI.Tools.Actions;
using Terms.UI.Tools.Extensions;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.Views;

namespace Terms.Windows.Tools
{
    public partial class ConnectionDetails
    {
        #region Private Read-Only Variables

        private readonly IXmlSettings m_settings;
        private readonly Connection m_connection;
        private readonly Credential m_credential;
        private readonly ListViewSettings m_listViewDetailsSettings;
        private readonly ListViewSettings m_listViewServicesSettings;
        private readonly ListViewSettings m_listViewProcessesSettings;
        private readonly ListViewSettings m_listViewDrivesSettings;

        #endregion

        #region Private Variables

        private bool m_updateWindowThreadRunning = true;
        private bool m_updateWindowDisdplayThreadRunning = true;
        private bool m_updateOnlyServices;
        private ManagementScope m_managementScope;

        #endregion

        public ConnectionDetails(IXmlSettings settings, Connection connection, Credential credential)
        {
            InitializeComponent();

            WindowLayout.Setup(this, WindowBorder, sideBorders: new List<Border> { bdSidePanel });

            m_settings = settings;
            m_connection = connection;
            m_credential = credential;

            XmlDocument xmlDocument = m_settings.GetDocument();

            m_listViewDetailsSettings = new ListViewSettings(m_settings, lstvDetails, $"{GetName}Detail", xmlDocument);
            m_listViewServicesSettings = new ListViewSettings(m_settings, lstvServices, $"{GetName}Service", xmlDocument);
            m_listViewProcessesSettings = new ListViewSettings(m_settings, lstvProcesses, $"{GetName}Process", xmlDocument);
            m_listViewDrivesSettings = new ListViewSettings(m_settings, lstvPhysicalDrives, $"{GetName}PhysicalDrive", xmlDocument);

            SetupDisplay(xmlDocument);
            SetupCollectionMode();
        }

        private static string GetName => string.Format(Settings.WindowNameFormat, nameof(ConnectionDetails), Settings.Window);

        private void SetupDisplay(XmlDocument xmlDocument)
        {
            int onlyShowUnavailableConnections = Convert.ToInt32(m_settings.Read(Settings.ConnectionDetailsWindow.ConnectionDetailsOptions, nameof(Settings.ConnectionDetailsWindow.OnlyShowTheRunningServices), Settings.ConnectionStatusesWindow.OnlyShowUnavailableConnections, xmlDocument));

            chkShowOnlyTheStoppedServices.IsChecked = onlyShowUnavailableConnections > 0;

            rbDetails.IsChecked = true;

            UpdateTabDisplay();
        }

        private void SetupCollectionMode()
        {
            m_updateWindowDisdplayThreadRunning = true;

            bRefresh.IsEnabled = false;

            if (!m_updateOnlyServices)
            {
                lstvDetails.Items.Clear();
                lstvProcesses.Items.Clear();
                lstvPhysicalDrives.Items.Clear();
            }

            lstvServices.Items.Clear();

            SetupWindow();
            SetupWindowUpdateThread();
            SetupWindowDisplayThread();
        }

        private void SetupWindow()
        {
            lblTitle.Text = string.Format(UIMessages.CollectingDetails, m_connection.Name, m_connection.Address);
        }

        #region Private "Window" Events

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_updateWindowThreadRunning = false;
            m_updateWindowDisdplayThreadRunning = false;

            XmlDocument xmlDocument = m_settings.GetDocument();

            m_settings.Write(Settings.ConnectionDetailsWindow.ConnectionDetailsOptions, nameof(Settings.ConnectionDetailsWindow.OnlyShowTheRunningServices), chkShowOnlyTheStoppedServices.IsReallyChecked().ToNumericString(), xmlDocument);

            m_listViewDetailsSettings.SetColumnWidths(xmlDocument);
            m_listViewServicesSettings.SetColumnWidths(xmlDocument);
            m_listViewProcessesSettings.SetColumnWidths(xmlDocument);
            m_listViewDrivesSettings.SetColumnWidths(xmlDocument);

            m_settings.SaveDocument(xmlDocument);
        }

        #endregion

        #region Private "Update Window Thread" Helpers

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
                    if (rbServices.IsReallyChecked())
                    {
                        bool disable = true;

                        if (lstvServices.SelectedItem != null && lstvServices.SelectedItem is WmiService wmiService)
                        {
                            bool isRunning = wmiService.State == UIMessages.Running;

                            mnuServices_StartService.IsEnabled = !isRunning;
                            mnuServices_StopService.IsEnabled = isRunning;
                            mnuServices_CopyName.IsEnabled = true;

                            disable = false;
                        }

                        if (disable)
                        {
                            mnuServices_StartService.IsEnabled = false;
                            mnuServices_StopService.IsEnabled = false;
                            mnuServices_CopyName.IsEnabled = false;
                        }
                    }
                });

                Thread.Sleep(50);
            }
        }

        #endregion

        #region Private "Update Window Display Thread" Helpers

        private void SetupWindowDisplayThread()
        {
            Thread thread = new(WindowDisplayThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void WindowDisplayThread()
        {
            while (m_updateWindowDisdplayThreadRunning)
            {
                try
                {
                    Connect();

                    if (!m_updateOnlyServices)
                    {
                        AddDetails();
                    }

                    AddServiceDetails();

                    if (!m_updateOnlyServices)
                    {
                        AddProcessDetails();
                        AddPhysicalDriveDetails();
                    }

                    BackgroundAction.Run(() =>
                    {
                        lblTitle.Text = string.Format(UIMessages.AllDetailsFor, m_connection.Name, m_connection.Address);
                        bRefresh.IsEnabled = true;
                    });
                }
                catch
                {
                    BackgroundAction.Run(() =>
                    {
                        lblTitle.Text = string.Format(UIMessages.UnableToGetDetails, m_connection.Name, m_connection.Address);
                        bRefresh.IsEnabled = true;
                    });
                }

                m_updateOnlyServices = false;
                m_updateWindowDisdplayThreadRunning = false;
                break;
            }
        }

        private void Connect()
        {
            if (m_managementScope == null)
            {
                m_managementScope = new ManagementScope($"\\\\{m_connection.Address}\\root\\cimv2", GetConnectionOptions());
                m_managementScope.Connect();
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

        private void AddDetails()
        {
            using (ManagementObjectCollection managementObjectCollection = GetObjectCollection("SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
                {
                    if (managementBaseObject is ManagementObject managementObject)
                    {
                        foreach (PropertyData propertyData in managementObject.Properties)
                        {
                            if (propertyData.Value != null)
                            {
                                string name = propertyData.Name;
                                string value;

                                switch (propertyData.Type)
                                {
                                    case CimType.String when propertyData.Value.GetType() == typeof(string[]):
                                        value = propertyData.Value.ToSeperatedString();
                                        break;

                                    case CimType.DateTime:
                                        DateTime dateTime = ManagementDateTimeConverter.ToDateTime(propertyData.Value.ToString());
                                        value = dateTime.ToLongDateString();
                                        break;

                                    default:
                                        value = propertyData.Value.ToString();
                                        break;
                                }

                                BackgroundAction.Run(() =>
                                {
                                    WmiDetail wmiDetail = new()
                                    {
                                        Name = name,
                                        Value = value
                                    };

                                    lstvDetails.Items.Add(wmiDetail);
                                });
                            }
                        }
                    }
                }
            }
        }

        private void AddServiceDetails()
        {
            BackgroundAction.Run(() =>
            {
                lblTitle.Text = string.Format(UIMessages.CollectingServices, m_connection.Name, m_connection.Address);
            });

            using (ManagementObjectCollection managementObjectCollection = GetObjectCollection("SELECT Name, Caption, State FROM Win32_Service"))
            {
                foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
                {
                    if (managementBaseObject is ManagementObject managementObject)
                    {
                        string name = managementObject["Name"].ToString();
                        string caption = managementObject["Caption"].ToString();
                        string state = managementObject["State"].ToString();

                        bool visible = true;

                        BackgroundAction.Run(() =>
                        {
                            if (chkShowOnlyTheStoppedServices.IsReallyChecked())
                            {
                                visible = state != UIMessages.Running;
                            }

                            WmiService wmiService = new()
                            {
                                Name = name,
                                Description = caption,
                                State = state,
                                Visibility = visible ? Visibility.Visible : Visibility.Collapsed
                            };

                            lstvServices.Items.Add(wmiService);
                        });
                    }
                }
            }
        }

        private void AddProcessDetails()
        {
            BackgroundAction.Run(() =>
            {
                lblTitle.Text = string.Format(UIMessages.CollectingProcesses, m_connection.Name, m_connection.Address);
            });

            using (ManagementObjectCollection managementObjectCollection = GetObjectCollection("SELECT ProcessId, Name FROM Win32_Process"))
            {
                foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
                {
                    if (managementBaseObject is ManagementObject managementObject)
                    {
                        string id = managementObject["ProcessId"].ToString();
                        string name = managementObject["Name"].ToString();

                        BackgroundAction.Run(() =>
                        {
                            WmiProcess wmiProcess = new()
                            {
                                Id = id,
                                Name = name
                            };

                            lstvProcesses.Items.Add(wmiProcess);
                        });
                    }
                }
            }
        }

        private void AddPhysicalDriveDetails()
        {
            BackgroundAction.Run(() =>
            {
                lblTitle.Text = string.Format(UIMessages.CollectingPhysicalDrives, m_connection.Name, m_connection.Address);
            });

            using (ManagementObjectCollection managementObjectCollection = GetObjectCollection("SELECT Name, VolumeName, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType = 3"))
            {
                foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
                {
                    if (managementBaseObject is ManagementObject managementObject)
                    {
                        string name = managementObject["Name"].ToString();
                        string volumeName = managementObject["VolumeName"].ToString();
                        string size = managementObject["Size"].ToString();
                        string freespace = managementObject["FreeSpace"].ToString();

                        if (long.TryParse(size, out long newSize))
                        {
                            size = newSize.ToFormattedSize();
                        }

                        if (long.TryParse(freespace, out long newFreespace))
                        {
                            freespace = newFreespace.ToFormattedSize();
                        }

                        BackgroundAction.Run(() =>
                        {
                            WmiDrive wmiDrive = new()
                            {
                                Name = name,
                                VolumeName = volumeName,
                                Size = size,
                                FreeSpace = freespace
                            };

                            lstvPhysicalDrives.Items.Add(wmiDrive);
                        });
                    }
                }
            }
        }

        private ManagementObjectCollection GetObjectCollection(string query)
        {
            ObjectQuery objectQuery = new(query);
            ManagementObjectSearcher managementObjectSearcher = new(m_managementScope, objectQuery);

            return managementObjectSearcher.Get();
        }

        #endregion

        #region Private "ListView" Events

        private void Details_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            ListViewOrdering listViewOrdering = new(lstvDetails, e);
            listViewOrdering.Sort();
        }

        private void Services_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            ListViewOrdering listViewOrdering = new(lstvServices, e);
            listViewOrdering.Sort();
        }

        private void Processes_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            ListViewOrdering listViewOrdering = new(lstvProcesses, e);
            listViewOrdering.Sort();
        }

        private void PhysicalDrives_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            ListViewOrdering listViewOrdering = new(lstvPhysicalDrives, e);
            listViewOrdering.Sort();
        }

        private void ListView_Details_OnKeyDown(object sender, KeyEventArgs e)
        {
            ListViewAction.FindOnKeydown(e.Key.ToString(), lstvDetails);
        }

        private void ListView_Services_OnKeyDown(object sender, KeyEventArgs e)
        {
            ListViewAction.FindOnKeydown(e.Key.ToString(), lstvServices);
        }

        private void ListView_Processes_OnKeyDown(object sender, KeyEventArgs e)
        {
            ListViewAction.FindOnKeydown(e.Key.ToString(), lstvProcesses);
        }

        private void ListView_PhysicalDrives_OnKeyDown(object sender, KeyEventArgs e)
        {
            ListViewAction.FindOnKeydown(e.Key.ToString(), lstvPhysicalDrives);
        }

        #endregion

        #region Private "Tab Display" Events

        private void Tab_OnChecked(object sender, RoutedEventArgs e)
        {
            UpdateTabDisplay();
        }

        private void UpdateTabDisplay()
        {
            if (gDetails != null)
            {
                gDetails.Visibility = Visibility.Collapsed;
                gServices.Visibility = Visibility.Collapsed;
                gProcesses.Visibility = Visibility.Collapsed;
                gPhysicalDrives.Visibility = Visibility.Collapsed;

                if (rbDetails.IsReallyChecked())
                {
                    gDetails.Visibility = Visibility.Visible;
                }
                else if (rbServices.IsReallyChecked())
                {
                    gServices.Visibility = Visibility.Visible;
                }
                else if (rbProcesses.IsReallyChecked())
                {
                    gProcesses.Visibility = Visibility.Visible;
                }
                else if (rbPhysicalDrives.IsReallyChecked())
                {
                    gPhysicalDrives.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region Private "Button" Events

        private void Button_Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            SetupCollectionMode();
        }

        #endregion

        #region Private "CheckBox" Events

        private void CheckBox_ShowOnlyTheStoppedServices_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            foreach (object serviceItem in lstvServices.Items)
            {
                WmiService wmiService = (WmiService) serviceItem;

                if (!chkShowOnlyTheStoppedServices.IsReallyChecked())
                {
                    wmiService.Visibility = Visibility.Visible;
                }
                else
                {
                    if (chkShowOnlyTheStoppedServices.IsReallyChecked() && wmiService.State != UIMessages.Running)
                    {
                        wmiService.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        wmiService.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        #endregion

        #region Private "Services" Area

        private void MenuItem_StartService_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvServices.SelectedIndex > -1 && lstvServices.SelectedItem is WmiService service)
            {
                HandleService(service.Name, true, service);
            }
        }

        private void MenuItem_StopService_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvServices.SelectedIndex > -1 && lstvServices.SelectedItem is WmiService service)
            {
                HandleService(service.Name, false, service);
            }
        }

        private void MenuItem_CopyName_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvServices.SelectedIndex > -1 && lstvServices.SelectedItem is WmiService service)
            {
                Clipboard.SetText(service.Name, TextDataFormat.Text);
            }
        }

        private void HandleService(string name, bool start, WmiService wmiService)
        {
            bool showOnlyTheStopServices = chkShowOnlyTheStoppedServices.IsReallyChecked();

            Thread thread = new(() =>
            {
                string command = start ? "StartService" : "StopService";
                string path = $"Win32_Service.Name='{name}'";

                Connect();

                bool updateFullList = false;

                using (ManagementObject managementObject = new(m_managementScope, new ManagementPath(path), null))
                {
                    try
                    {
                        managementObject.InvokeMethod(command, null, null);

                        wmiService.State = start ? UIMessages.Running : UIMessages.Stopped;

                        if (start && showOnlyTheStopServices)
                        {
                            wmiService.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch
                    {
                        updateFullList = true;
                    }
                }

                if (updateFullList)
                {
                    m_updateOnlyServices = true;

                    BackgroundAction.Run(SetupCollectionMode);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        #endregion
    }
}