using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Terms.Tools.Extensions;
using Terms.Tools.Settings.Interfaces;
using Terms.Tools.Windows;
using Terms.UI.Tools;
using Terms.UI.Tools.Actions;
using Terms.UI.Tools.Controls;
using Terms.UI.Tools.Data;
using Terms.UI.Tools.Models;
using Terms.UI.Tools.Shell;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.ViewModels.Storage;
using Terms.UI.Tools.Views;
using Terms.Windows.Display;
using Terms.Windows.List;
using Terms.Windows.Management;
using Terms.Windows.Report;
using Terms.Windows.Tools;

namespace Terms
{
    public partial class Main
    {
        #region Private Constants

        private const int UpdateWindowInterval = 50;

        #endregion

        #region Private Read-Only Variables

        private readonly IXmlSettings m_settings;
        private readonly WindowDisplay m_windowDisplay;
        private readonly WindowPosition m_windowPosition;
        private readonly ListViewSettings m_groupListViewSettings;
        private readonly ListViewSettings m_connectionsListViewSettings;
        private readonly ListViewSettings m_runningListViewSettings;
        private readonly WindowGroupedConnections m_windowGroupedConnections;
        private readonly FilenameDialog m_filenameDialog;
        private readonly Credentials m_credentials;
        private readonly XmlDocument m_xmlDocument;
        private readonly MstscProcesses m_mstscProcesses;

        #endregion

        #region Private Variables

        private bool m_updateWindowThreadRunning = true;
        private Search m_searchWindow;
        private bool m_searchWasRun;
        private bool m_showConfirmationMessagesForFileActions;
        private bool m_updateLastAccessedForPingedConnections;
        private bool m_updateLastAccessedWhenConnectionsAreShutdownOrRestarted;
        private bool m_minimizeMainWindowWhenAConnectionIsOpened;
        private bool m_rememberTheLastPickedUserCredentialsForConnections;
        private bool m_allowUserCredentialsPickingWhenOpeningConnections;
        private bool m_showConnectionsOpenInTitleBarWhenAvailable;
        private bool m_showConfirmationMessageBeforeClosingMainWindow;
        private bool m_minimizeAllOtherConnectionsWhenFocusingConnection;
        private TextFieldInput m_connectionAddress;

        #endregion

        public Main(IXmlSettings settings, XmlDocument xmlDocument, bool fadeInOut)
        {
            InitializeComponent();

            m_settings = settings;
            m_xmlDocument = xmlDocument;
            m_windowPosition = new WindowPosition(this, m_settings, Width, Height, GetName);
            m_groupListViewSettings = new ListViewSettings(m_settings, lstvGroups, $"{GetName}Groups", m_xmlDocument);
            m_connectionsListViewSettings = new ListViewSettings(m_settings, lstvConnections, $"{GetName}Connections", m_xmlDocument);
            m_runningListViewSettings = new ListViewSettings(m_settings, lstvRunning, $"{GetName}Running", m_xmlDocument);
            m_windowGroupedConnections = new WindowGroupedConnections(lstvGroups, lstvConnections);
            m_filenameDialog = new FilenameDialog(m_windowGroupedConnections);
            m_credentials = new Credentials();
            m_mstscProcesses = new MstscProcesses(lstvRunning);

            m_windowDisplay = new WindowDisplay(this)
            {
                OnBeforeWindowShown = InitializeBeforeWindowShownActions,
                OnAfterWindowShown = InitializeAfterWindowShownActions,
                IncrimentOpacity = fadeInOut
            };

            LockWindowActions.JustMaximized(this);

            m_credentials.Load();
            m_windowGroupedConnections.Load(afterLoadingAction: InitializeSettings);

            InitializeDisplaySettings(m_xmlDocument);

            SetupWindowUpdateThread();

            BackgroundAction.Run(() => m_windowPosition.Get(true, m_xmlDocument));
        }

        private static string GetName => string.Format(Settings.WindowNameFormat, nameof(Main), Settings.Window);

        private void InitializeSettings()
        {
            int groupSectionWidth = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.GroupSectionWidth), Settings.MainWindow.GroupSectionWidth, m_xmlDocument));
            int lastSelectedGroup = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.LastSelectedGroup), Settings.MainWindow.LastSelectedGroup, m_xmlDocument));
            int lastSelectedConnection = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.LastSelectedConnection), Settings.MainWindow.LastSelectedConnection, m_xmlDocument));
            string previousOpenNewConnectionEntered = m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.PreviousOpenNewConnectionEntered), Settings.MainWindow.PreviousOpenNewConnectionEntered, m_xmlDocument);

            cdGroupSection.Width = new GridLength(groupSectionWidth);
            txtConnectionAddress.Text = previousOpenNewConnectionEntered;

            if (lastSelectedGroup > -1)
            {
                lstvGroups.SelectedIndex = lastSelectedGroup;

                if (lastSelectedConnection > -1)
                {
                    lstvConnections.SelectedIndex = lastSelectedConnection;
                }
            }

            m_connectionAddress = new TextFieldInput(txtConnectionAddress)
            {
                TextChanged = ConnectionAddressTextChanged,
                PlaceHolder = Terms.Resources.UIMessages.OpenConnectionPlaceholder
            };

            IsInitialised = true;
        }

        private void InitializeBeforeWindowShownActions()
        {
            if (lstvGroups.SelectedIndex > -1)
            {
                ListViewAction.FocusSelectedItem(lstvGroups, lstvGroups.SelectedIndex);

                if (lstvConnections.SelectedIndex > -1)
                {
                    ListViewAction.FocusSelectedItem(lstvConnections, lstvConnections.SelectedIndex);
                }
            }
        }

        private void InitializeAfterWindowShownActions()
        {
            int checkToSeeIfNewUpdatesAreAvailable = Convert.ToInt32(m_settings.Read(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.CheckToSeeIfNewUpdatesAreAvailable), Settings.MainWindow.CheckToSeeIfNewUpdatesAreAvailable, m_xmlDocument));

            if (checkToSeeIfNewUpdatesAreAvailable > 0 && IsVisible)
            {
                About.CheckForUpdates(this);
            }
        }

        public void InitializeDisplaySettings(XmlDocument xmlDocument)
        {
            int showInTaskBar = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowInTaskBar), Settings.MainWindow.ShowInTaskBar, xmlDocument));
            int showConfirmationMessagesForFileActions = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowConfirmationMessagesForFileActions), Settings.MainWindow.ShowConfirmationMessagesForFileActions, xmlDocument));
            int minimizeMainWindowWhenAConnectionIsOpened = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.MinimizeMainWindowWhenAConnectionIsOpened), Settings.MainWindow.MinimizeMainWindowWhenAConnectionIsOpened, xmlDocument));
            int showOpenNewConnectionArea = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowOpenNewConnectionArea), Settings.MainWindow.ShowOpenNewConnectionArea, xmlDocument));
            int allowUserCredentialsPickingWhenOpeningConnections = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.AllowUserCredentialsPickingWhenOpeningConnections), Settings.MainWindow.AllowUserCredentialsPickingWhenOpeningConnections, xmlDocument));
            int rememberTheLastPickedUserCredentialsForConnections = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.RememberTheLastPickedUserCredentialsForConnections), Settings.MainWindow.RememberTheLastPickedUserCredentialsForConnections, xmlDocument));
            int showConnectionManagementButtonsInTheTitleBar = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowConnectionManagementButtonsInTheTitleBar), Settings.MainWindow.ShowConnectionManagementButtonsInTheTitleBar, xmlDocument));
            int showConnectionsOpenInTitleBarWhenAvailable = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowConnectionsOpenInTitleBarWhenAvailable), Settings.MainWindow.ShowConnectionsOpenInTitleBarWhenAvailable, xmlDocument));
            int runningSectionWidth = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.RunningSectionWidth), Settings.MainWindow.RunningSectionWidth, xmlDocument));

            int showTheRunningConnectionsSection = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Connections, nameof(Settings.MainWindow.ShowTheRunningConnectionsSection), Settings.MainWindow.ShowTheRunningConnectionsSection, xmlDocument));
            int showConfirmationMessageBeforeClosingMainWindow = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Connections, nameof(Settings.MainWindow.ShowConfirmationMessageBeforeClosingMainWindow), Settings.MainWindow.ShowConfirmationMessageBeforeClosingMainWindow, xmlDocument));
            int minimizeAllOtherConnectionsWhenFocusingConnection = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Connections, nameof(Settings.MainWindow.MinimizeAllOtherConnectionsWhenFocusingConnection), Settings.MainWindow.MinimizeAllOtherConnectionsWhenFocusingConnection, xmlDocument));
            int updateLastAccessedForPingedConnections = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Connections, nameof(Settings.MainWindow.UpdateLastAccessedForPingedConnections), Settings.MainWindow.UpdateLastAccessedForPingedConnections, xmlDocument));
            int updateLastAccessedWhenConnectionsAreShutdownOrRestarted = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Connections, nameof(Settings.MainWindow.UpdateLastAccessedWhenConnectionsAreShutdownOrRestarted), Settings.MainWindow.UpdateLastAccessedWhenConnectionsAreShutdownOrRestarted, xmlDocument));

            int fadeMainWindowInOutOnStartupShutdown = Convert.ToInt32(m_settings.Read(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.FadeMainWindowInOutOnStartupShutdown), Settings.MainWindow.FadeMainWindowInOutOnStartupShutdown, xmlDocument));

            ShowInTaskbar = showInTaskBar > 0;
            m_showConfirmationMessagesForFileActions = showConfirmationMessagesForFileActions > 0;
            m_minimizeMainWindowWhenAConnectionIsOpened = minimizeMainWindowWhenAConnectionIsOpened > 0;
            rdOpenConnection.Height = showOpenNewConnectionArea > 0 ? new GridLength(33) : new GridLength(0);
            m_allowUserCredentialsPickingWhenOpeningConnections = allowUserCredentialsPickingWhenOpeningConnections > 0;
            m_rememberTheLastPickedUserCredentialsForConnections = rememberTheLastPickedUserCredentialsForConnections > 0;
            m_updateLastAccessedForPingedConnections = updateLastAccessedForPingedConnections > 0;
            m_updateLastAccessedWhenConnectionsAreShutdownOrRestarted = updateLastAccessedWhenConnectionsAreShutdownOrRestarted > 0;
            spConnectionManagementButtons.Visibility = showConnectionManagementButtonsInTheTitleBar > 0 ? Visibility.Visible : Visibility.Collapsed;
            m_showConnectionsOpenInTitleBarWhenAvailable = showConnectionsOpenInTitleBarWhenAvailable > 0;
            m_showConfirmationMessageBeforeClosingMainWindow = showConfirmationMessageBeforeClosingMainWindow > 0 && MstscProcesses.IsEnvironmentValidForTracking;
            m_minimizeAllOtherConnectionsWhenFocusingConnection = minimizeAllOtherConnectionsWhenFocusingConnection > 0;
            m_windowDisplay.IncrimentOpacity = fadeMainWindowInOutOnStartupShutdown > 0;

            if (MstscProcesses.IsEnvironmentValidForTracking && showTheRunningConnectionsSection > 0)
            {
                cdRunningSection.Width = new GridLength(runningSectionWidth);
                cdRunningSection.MinWidth = 200;
                gsRunningSplitter.Visibility = Visibility.Visible;
            }
            else
            {
                cdRunningSection.Width = new GridLength(0);
                cdRunningSection.MinWidth = 0;
                gsRunningSplitter.Visibility = Visibility.Collapsed;

                m_showConfirmationMessageBeforeClosingMainWindow = false;
            }

            InitializeMinimizeButton();
        }

        private void InitializeMinimizeButton()
        {
            string minimizeText = Terms.Resources.UIMessages.Minimize;

            if (!ShowInTaskbar)
            {
                minimizeText = Terms.Resources.UIMessages.MinimizeToTray;
            }

            bMinimize.ToolTip = minimizeText;
            mnuMinimize.Header = $"_{minimizeText}";
        }

        public bool DoesGroupExist(string name, int ignoreIndex = -1)
        {
            return ListViewAction.Find(name, lstvGroups, ignoreIndex);
        }

        public bool DoesConnectionExist(string name, int ignoreIndex = -1)
        {
            return ListViewAction.Find(name, lstvConnections, ignoreIndex);
        }

        public void AddGroup(Group group, int selectedIndex = -1)
        {
            if (selectedIndex > -1)
            {
                Group actualGroupViewModel = (Group) lstvGroups.Items[selectedIndex];
                actualGroupViewModel.Update(group);

                SetAllowMutipleConnectionManagementMode(group);
            }
            else
            {
                lstvGroups.Items.Add(group);
            }
        }

        public void AddConnection(Connection connection, int selectedIndex = -1)
        {
            Group group = (Group) lstvGroups.SelectedItem;

            if (selectedIndex > -1)
            {
                Connection actualConnectionViewModel = (Connection) lstvConnections.SelectedItem;
                actualConnectionViewModel.Update(connection);
            }
            else
            {
                lstvConnections.Items.Add(connection);

                group.Connections.Add(connection);
            }

            group.ConnectionsChanged();
        }

        public void ChangeAllPasswords(string password)
        {
            foreach (object item in lstvGroups.Items)
            {
                Group group = (Group) item;

                if (group.AllowAllPasswordsToBeChanged)
                {
                    foreach (Connection connection in group.Connections)
                    {
                        if (!connection.AskForCredentials)
                        {
                            connection.Password = password;
                        }
                    }
                }
            }
        }

        public void ClearAllLastAccessedHistory()
        {
            foreach (object item in lstvGroups.Items)
            {
                Group group = (Group) item;

                foreach (Connection connection in group.Connections)
                {
                    connection.ResetLastAccessed();
                }
            }
        }

        public void ClearAllConnectionPorts()
        {
            foreach (object item in lstvGroups.Items)
            {
                Group group = (Group) item;

                foreach (Connection connection in group.Connections)
                {
                    connection.ResetPort();
                }
            }
        }

        public void RemovedConnectionsByName(List<string> connectionNames)
        {
            int selectedIndex = lstvGroups.SelectedIndex;
            if (selectedIndex > 0)
            {
                Group group = (Group) lstvGroups.Items[selectedIndex];

                foreach (string connectionName in connectionNames)
                {
                    for (int connectionViewModelIndex = lstvConnections.Items.Count - 1; connectionViewModelIndex > -1; connectionViewModelIndex--)
                    {
                        Connection connection = (Connection) lstvConnections.Items[connectionViewModelIndex];

                        if (connection.Name == connectionName)
                        {
                            lstvConnections.Items.RemoveAt(connectionViewModelIndex);
                            group.Connections.RemoveAt(connectionViewModelIndex);
                            break;
                        }
                    }
                }

                group.ConnectionsChanged();
            }
        }

        public bool IsInitialised { get; set; }

        #region Private Window Update Thread

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
                BackgroundAction.Run(() =>
                {
                    Navigation_UpdateTitleBarButtons();
                    Navigation_UpdateGroupsContextMenu();
                    Navigation_UpdateConnectionsContextMenu();
                    Navigation_UpdateRunningContextMenu();
                    Navigation_UpdateOpenConnectionArea();

                    UpdateWindowState();
                    UpdateWindowTitle();

                    m_windowDisplay.UpdateWindow();
                });

                Thread.Sleep(UpdateWindowInterval);
            }
        }

        private void Navigation_UpdateTitleBarButtons()
        {
            bool button_IsGroupItemsAvailable = lstvGroups.Items.Count > 0;
            bool button_IsAtLeastOneGroupEnabledForAllPasswordChanging = IsAtLeastOneGroupEnabledForAllPasswordChanging();
            bool button_IsAtLeastOneConnectionAvailable = IsAtLeastOneConnectionsAvailable();

            bNew.IsEnabled = button_IsGroupItemsAvailable;
            bSaveAs.IsEnabled = button_IsGroupItemsAvailable;
            bSearch.IsEnabled = button_IsGroupItemsAvailable;
            bChangeAllPasswords.IsEnabled = button_IsGroupItemsAvailable && button_IsAtLeastOneGroupEnabledForAllPasswordChanging;
            bCheckAllConnectionStatuses.IsEnabled = button_IsAtLeastOneConnectionAvailable;
        }

        private void Navigation_UpdateGroupsContextMenu()
        {
            bool group_IsConnectionsAvailable = lstvConnections.Items.Count > 0;
            bool group_IsSelected = lstvGroups.SelectedIndex > -1;
            bool group_IsTopSelected = lstvGroups.SelectedIndex <= 0;
            bool group_IsBottomSelected = lstvGroups.SelectedIndex >= lstvGroups.Items.Count - 1;

            mnuGroup_OpenAll.IsEnabled = group_IsConnectionsAvailable;
            mnuGroup_MoveUp.IsEnabled = group_IsSelected && !group_IsTopSelected;
            mnuGroup_MoveDown.IsEnabled = group_IsSelected && !group_IsBottomSelected;
            mnuGroup_Delete.IsEnabled = group_IsSelected;
            mnuGroup_Edit.IsEnabled = group_IsSelected;
        }

        private void Navigation_UpdateConnectionsContextMenu()
        {
            bool connection_IsListAvailable = lstvConnections.Items.Count > 0;
            bool connection_IsSelected = lstvConnections.SelectedIndex > -1;
            bool connection_IsTopSelected = lstvConnections.SelectedIndex <= 0;
            bool connection_IsBottonSelected = lstvConnections.SelectedIndex >= lstvConnections.Items.Count - 1;
            bool connection_IsMoreThanOneSelected = lstvConnections.SelectedItems.Count > 1;
            bool connection_IsOnlyOneSelected = connection_IsSelected && !connection_IsMoreThanOneSelected;
            bool connection_IsSelectedItemsEnabled = AreAllSelectedConnectionsEnabled();
            bool connection_IsAllItemsSelected = lstvConnections.SelectedItems.Count > 0 && lstvConnections.SelectedItems.Count == lstvConnections.Items.Count;
            bool connaction_IsAllowMultipleConnectionManagementEnabled = lstvConnections.SelectionMode == SelectionMode.Extended;

            mnuConnection_SelectionSeparator.Visibility = connaction_IsAllowMultipleConnectionManagementEnabled ? Visibility.Visible : Visibility.Collapsed;
            mnuConnection_SelectAll.Visibility = connaction_IsAllowMultipleConnectionManagementEnabled ? Visibility.Visible : Visibility.Collapsed;
            mnuConnection_SelectNone.Visibility = connaction_IsAllowMultipleConnectionManagementEnabled ? Visibility.Visible : Visibility.Collapsed;
            mnuConnection_SelectInverse.Visibility = connaction_IsAllowMultipleConnectionManagementEnabled ? Visibility.Visible : Visibility.Collapsed;

            mnuConnection_Open.IsEnabled = connection_IsSelected && connection_IsSelectedItemsEnabled;
            mnuConnection_MoveUp.IsEnabled = connection_IsSelected && !connection_IsTopSelected && !connection_IsMoreThanOneSelected;
            mnuConnection_MoveDown.IsEnabled = connection_IsSelected && !connection_IsBottonSelected && !connection_IsMoreThanOneSelected;
            mnuConnection_Copy.IsEnabled = connection_IsOnlyOneSelected;
            mnuConnection_Delete.IsEnabled = connection_IsSelected;
            mnuConnection_DeleteAll.IsEnabled = connection_IsListAvailable;
            mnuConnection_Move.IsEnabled = connection_IsSelected;
            mnuConnection_Ping.IsEnabled = connection_IsOnlyOneSelected && connection_IsSelectedItemsEnabled;
            mnuConnection_Duplicate.IsEnabled = connection_IsOnlyOneSelected;
            mnuConnection_Options.IsEnabled = connection_IsListAvailable;
            mnuConnection_Details.IsEnabled = connection_IsOnlyOneSelected && connection_IsSelectedItemsEnabled && m_credentials.Count > 0;
            mnuConnection_Statuses.IsEnabled = connection_IsListAvailable;
            mnuConnection_ShutdownRestart.IsEnabled = connection_IsSelected && connection_IsSelectedItemsEnabled;
            mnuConnection_ServiceManagement.IsEnabled = connection_IsSelected && connection_IsSelectedItemsEnabled;
            mnuConnection_SelectAll.IsEnabled = connaction_IsAllowMultipleConnectionManagementEnabled && connection_IsListAvailable && !connection_IsAllItemsSelected;
            mnuConnection_SelectNone.IsEnabled = connaction_IsAllowMultipleConnectionManagementEnabled && connection_IsListAvailable && connection_IsSelected;
            mnuConnection_SelectInverse.IsEnabled = connaction_IsAllowMultipleConnectionManagementEnabled && connection_IsListAvailable;
            mnuConnection_Edit.IsEnabled = connection_IsOnlyOneSelected;
        }

        private void Navigation_UpdateRunningContextMenu()
        {
            if (Math.Abs(cdRunningSection.Width.Value) > 0)
            {
                bool running_IsListAvailable = lstvRunning.Items.Count > 0;
                bool running_IsSelected = lstvRunning.SelectedIndex > -1;
                bool running_isMoreThanOneSelected = lstvRunning.SelectedItems.Count > 1;
                bool running_IsOnlyOneSelected = running_IsSelected && !running_isMoreThanOneSelected;
                bool running_IsAllItemsSelected = lstvRunning.SelectedItems.Count > 0 && lstvRunning.SelectedItems.Count == lstvRunning.Items.Count;

                mnuRunning_Focus.IsEnabled = running_IsOnlyOneSelected;
                mnuRunning_SetAsNormal.IsEnabled = running_IsSelected;
                mnuRunning_SetAsMinimized.IsEnabled = running_IsSelected;
                mnuRunning_SetAsMaximized.IsEnabled = running_IsSelected;
                mnuRunning_SelectAll.IsEnabled = running_IsListAvailable && !running_IsAllItemsSelected;
                mnuRunning_SelectNone.IsEnabled = running_IsListAvailable && running_IsSelected;
                mnuRunning_SelectInverse.IsEnabled = running_IsListAvailable;
                mnuRunning_Close.IsEnabled = running_IsSelected;
            }
        }

        private void Navigation_UpdateOpenConnectionArea()
        {
            if (Math.Abs(rdOpenConnection.Height.Value) > 0)
            {
                bool enableAddButton = false;
                bool enablePingButton = false;
                bool enableOpenConnectionButton = false;

                if (!string.IsNullOrEmpty(m_connectionAddress.Text))
                {
                    enableAddButton = lstvConnections.Items.Cast<Connection>().All(connection => !string.Equals(connection.Address, m_connectionAddress.Text, StringComparison.CurrentCultureIgnoreCase));
                    enablePingButton = lstvConnections.Items.Cast<Connection>().All(connection => !string.Equals(connection.Address, m_connectionAddress.Text, StringComparison.CurrentCultureIgnoreCase) || connection.Enabled);
                    enableOpenConnectionButton = enablePingButton;
                }

                bPing.IsEnabled = enablePingButton;
                bAddConnection.IsEnabled = enableAddButton;
                bOpenConnection.IsEnabled = enableOpenConnectionButton;
            }
        }

        private void UpdateWindowState()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void UpdateWindowTitle()
        {
            bool areConnectionsOpen = m_mstscProcesses.AreProcessesRunning && m_showConnectionsOpenInTitleBarWhenAvailable;

            string formattedTitle = $"{Terms.Resources.UIMessages.Terms} {Terms.Resources.UIMessages.OpenBracket}{Terms.Resources.UIMessages.ConnectionsOpen}{Terms.Resources.UIMessages.CloseBracket}";

            string title = areConnectionsOpen
                ? formattedTitle
                : Terms.Resources.UIMessages.Terms;

            if (areConnectionsOpen)
            {
                rTermsTitleOpen.Text = Terms.Resources.UIMessages.OpenBracket;
                rTermsTitleConnectionsOpen.Text = Terms.Resources.UIMessages.ConnectionsOpen;
                rTermsTitleClose.Text = Terms.Resources.UIMessages.CloseBracket;
            }
            else
            {
                rTermsTitleOpen.Text = string.Empty;
                rTermsTitleConnectionsOpen.Text = string.Empty;
                rTermsTitleClose.Text = string.Empty;
            }

            Title = title;
        }

        private bool IsAtLeastOneGroupEnabledForAllPasswordChanging()
        {
            return lstvGroups.Items.Cast<Group>().Any(group => group.AllowAllPasswordsToBeChanged);
        }

        private bool IsAtLeastOneConnectionsAvailable()
        {
            return lstvGroups.Items.Cast<Group>().Any(group => group.Connections.Count > 0);
        }

        private bool AreAllSelectedConnectionsEnabled()
        {
            return lstvConnections.SelectedItems.Cast<Connection>().All(connection => connection.Enabled);
        }

        #endregion

        #region Private "Search" Helpers

        public void InitializeSearch()
        {
            int wasOpenedOnExit = Convert.ToInt32(m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.WasOpenedOnExit), Settings.SearchWindow.WasOpenedOnExit));

            if (wasOpenedOnExit > 0)
            {
                OpenSearch(true);
            }
        }

        private void OpenSearch(bool runSearch = false)
        {
            if (m_searchWindow == null)
            {
                m_searchWindow = new Search(m_settings, lstvGroups, lstvConnections, this)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                m_searchWindow.ShowWindow(runSearch);

                if (runSearch)
                {
                    Focus();
                }
            }
        }

        public void ClearSearchUsage()
        {
            if (m_searchWindow != null)
            {
                m_searchWindow = null;
                m_searchWasRun = false;
            }
        }

        public void SetSearchAsRun()
        {
            m_searchWasRun = true;
        }

        #endregion

        #region Private "Title Bar" Events

        private void Title_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
            {
                DragMove();

                m_windowPosition.Changed = true;
            }
            else if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                double maximumHeight = SystemParameters.WorkArea.Height;

                if (Left + Width < SystemParameters.WorkArea.Width / 2)
                {
                    Left = 0;
                    Top = 0;
                    Height = maximumHeight;
                }
                else
                {
                    double maximumWidth = SystemParameters.WorkArea.Width;

                    Left = maximumWidth - Width;
                    Top = 0;
                    Height = maximumHeight;
                }

                m_windowPosition.Changed = true;
            }
        }

        #endregion

        #region Private "Title Bar Button" Events

        private void Button_New_OnClick(object sender, RoutedEventArgs e)
        {
            MessageQuestionResult messageQuestionResult = GetConfirmationFromMessage(
                Terms.Resources.UIMessages.NewConfirmation, 
                m_showConfirmationMessagesForFileActions,
                Settings.MainWindow.Display, 
                nameof(Settings.MainWindow.ShowConfirmationMessagesForFileActions));

            if (messageQuestionResult.Result)
            {
                lstvGroups.Items.Clear();
                lstvConnections.Items.Clear();
            }

            m_showConfirmationMessagesForFileActions = messageQuestionResult.SettingResult;
        }

        private void Button_Open_OnClick(object sender, RoutedEventArgs e)
        {
            MessageQuestionResult messageQuestionResult = GetConfirmationFromMessage(
                Terms.Resources.UIMessages.OpenConfirmation,
                m_showConfirmationMessagesForFileActions && lstvGroups.Items.Count > 0,
                Settings.MainWindow.Display, 
                nameof(Settings.MainWindow.ShowConfirmationMessagesForFileActions));

            if (messageQuestionResult.Result)
            {
                m_filenameDialog.Open(Terms.Resources.Dialog.SupportedFileFilter, Terms.Resources.Dialog.Open);
            }

            m_showConfirmationMessagesForFileActions = messageQuestionResult.SettingResult;
        }

        private void Button_SaveAs_OnClick(object sender, RoutedEventArgs e)
        {
            m_filenameDialog.Save(Terms.Resources.Dialog.SupportedFileFilter, Terms.Resources.Dialog.SaveAs);
        }

        private void Button_Credentials_OnClick(object sender, RoutedEventArgs e)
        {
            EditCredentials credentials = new EditCredentials(m_settings, m_credentials, m_filenameDialog)
            {
                Topmost = Topmost,
                Owner = this
            };

            credentials.ShowDialog();
        }

        private void Button_Search_OnClick(object sender, RoutedEventArgs e)
        {
            OpenSearch();
        }

        private void Button_ImportFromTerminals_OnClick(object sender, RoutedEventArgs e)
        {
            List<Group> groups = lstvGroups.Items.Cast<Group>().ToList();

            TerminalsXml terminalsXml = new TerminalsXml();

            if (terminalsXml.Read(Terms.Resources.Dialog.TerminalFileFilter, Terms.Resources.Dialog.OpenTerminalsFile, groups))
            {
                int lastSelectedGroupIndex = lstvGroups.SelectedIndex;
                int lastSelectedConnectionIndex = lstvConnections.SelectedIndex;

                lstvGroups.Items.Clear();
                lstvConnections.Items.Clear();

                foreach (Group group in groups)
                {
                    lstvGroups.Items.Add(group);
                }

                lstvGroups.SelectedIndex = lastSelectedGroupIndex;
                lstvConnections.SelectedIndex = lastSelectedConnectionIndex;
            }
        }

        private void Button_Options_OnClick(object sender, RoutedEventArgs e)
        {
            Options options = new Options(m_settings, this)
            {
                Topmost = Topmost,
                Owner = this
            };

            options.ShowDialog();
        }

        private void Button_ChangeAllPasswords_OnClick(object sender, RoutedEventArgs e)
        {
            ChangePasswords changePasswords = new ChangePasswords(this)
            {
                Topmost = Topmost,
                Owner = this
            };

            changePasswords.ShowDialog();
        }

        private void Button_OpenComputerManagement_OnClick(object sender, RoutedEventArgs e)
        {
            Processes.Start("compmgmt.msc");
        }

        private void Button_CheckAllConnectionStatuses_OnClick(object sender, RoutedEventArgs e)
        {
            List<Connection> connections = new List<Connection>();

            foreach (object item in lstvGroups.Items)
            {
                Group group = (Group)item;

                foreach (Connection connection in group.Connections)
                {
                    if (connection.Enabled)
                    {
                        Connection newConnectionViewModel = new Connection();
                        newConnectionViewModel.Update(connection);
                        newConnectionViewModel.Name = $"{group.Name} > {connection.Name}";

                        connections.Add(newConnectionViewModel);
                    }
                }
            }

            ConnectionStatuses connectionStatuses = new ConnectionStatuses(m_settings, connections, this, false)
            {
                Topmost = Topmost,
                Owner = this
            };

            connectionStatuses.ShowDialog();
        }

        private void Button_About_OnClick(object sender, RoutedEventArgs e)
        {
            About about = new About(m_settings)
            {
                Topmost = Topmost,
                Owner = this
            };

            about.ShowDialog();

            if (WindowState != WindowState.Minimized && IsVisible)
            {
                BringIntoView();
                Focus();
            }
        }

        private void Button_Minimize_OnClick(object sender, RoutedEventArgs e)
        {
            MinimizeWindow();
        }

        private void Button_Close_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvRunning.Items.Count == 0)
            {
                m_windowDisplay.Close();
            }
            else
            {
                MessageQuestionResult messageQuestionResult = GetConfirmationFromMessage(
                    Terms.Resources.UIMessages.CloseMainWindowConfirmation,
                    m_showConfirmationMessageBeforeClosingMainWindow,
                    Settings.MainWindow.Connections, 
                    nameof(Settings.MainWindow.ShowConfirmationMessageBeforeClosingMainWindow));

                if (messageQuestionResult.Result)
                {
                    m_windowDisplay.Close();
                }

                m_showConfirmationMessageBeforeClosingMainWindow = messageQuestionResult.SettingResult;
            }
        }

        private void MinimizeWindow()
        {
            WindowState = WindowState.Minimized;
        }

        #endregion

        #region Private "Window" Events

        private void Window_OnActivated(object sender, EventArgs e)
        {
            WindowBorder.BorderBrush = WindowLayout.BorderActivatedColor;
            InternalWindowBorder.BorderBrush = WindowLayout.BorderActivatedColor;
            lblTitle.Background = WindowLayout.BorderActivatedColor;
        }

        private void Window_OnDeactivated(object sender, EventArgs e)
        {
            WindowBorder.BorderBrush = WindowLayout.BorderDeactivatedColor;
            InternalWindowBorder.BorderBrush = WindowLayout.BorderDeactivatedColor;
            lblTitle.Background = WindowLayout.BorderDeactivatedColor;
        }

        private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (KeyStroke.IsControlKey(Key.N) && bNew.IsEnabled)
            {
                Button_New_OnClick(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.O) && bOpen.IsEnabled)
            {
                Button_Open_OnClick(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.S) && bSaveAs.IsEnabled)
            {
                Button_SaveAs_OnClick(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.Up) && mnuGroup_MoveUp.IsEnabled)
            {
                MenuItem_Group_MoveUp_OnClick(sender, null);
                e.Handled = true;
            }
            else if (KeyStroke.IsControlKey(Key.Down) && mnuGroup_MoveDown.IsEnabled)
            {
                MenuItem_Group_MoveDown_OnClick(sender, null);
                e.Handled = true;
            }
            else if (KeyStroke.IsControlKey(Key.F) && bSearch.IsEnabled)
            {
                Button_Search_OnClick(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.A) && mnuConnection_SelectAll.IsEnabled)
            {
                MenuItem_Connection_SelectAll_OnClick(sender, null);
            }
            else if ((KeyStroke.IsControlKey(Key.Z) || Keyboard.IsKeyDown(Key.Escape)) && mnuConnection_SelectNone.IsEnabled)
            {
                MenuItem_Connection_SelectNone_OnClick(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.I) && mnuConnection_SelectInverse.IsEnabled)
            {
                MenuItem_Connection_SelectInverse_OnClick(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.Enter) && mnuGroup_OpenAll.IsEnabled)
            {
                MenuItem_Group_OpenAll_OnClick(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.Delete) && mnuGroup_Delete.IsEnabled)
            {
                MenuItem_Group_Delete_OnClick(sender, null);
            }
            else if (KeyStroke.IsShiftKey(Key.Up) && mnuConnection_MoveUp.IsEnabled)
            {
                MenuItem_Connection_MoveUp_OnClick(sender, null);
                e.Handled = true;
            }
            else if (KeyStroke.IsShiftKey(Key.Down) && mnuConnection_MoveDown.IsEnabled)
            {
                MenuItem_Connection_MoveDown_OnClick(sender, null);
                e.Handled = true;
            }
            else if (Keyboard.IsKeyDown(Key.Enter))
            {
                if (m_connectionAddress.IsFocused)
                {
                    if (bOpenConnection.IsEnabled)
                    {
                        Button_OpenConnectionArea_OpenConnection_OnClick(sender, null);
                    }
                }
                else
                {
                    if (mnuConnection_Open.IsEnabled)
                    {
                        MenuItem_Connection_Open_OnClick(sender, null);
                    }
                }
            }
            else if (Keyboard.IsKeyDown(Key.Delete) && mnuConnection_Delete.IsEnabled)
            {
                MenuItem_Connection_Delete_OnClick(sender, null);
            }
            else if (KeyStroke.IsAltKey(Key.Space))
            {
                e.Handled = true;
            }
        }

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_updateWindowThreadRunning = false;

            if (m_searchWindow != null)
            {
                m_searchWindow.DoFullSaveofSettings = false;
                m_searchWindow.Close();
            }

            XmlDocument xmlDocument = m_settings.GetDocument();

            m_windowPosition.Set(xmlDocument);
            m_groupListViewSettings.SetColumnWidths(xmlDocument);
            m_connectionsListViewSettings.SetColumnWidths(xmlDocument);
            m_runningListViewSettings.SetColumnWidths(xmlDocument);

            m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.GroupSectionWidth), cdGroupSection.Width.ToString(), xmlDocument);
            m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.LastSelectedGroup), lstvGroups.SelectedIndex.ToString(), xmlDocument);
            m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.LastSelectedConnection), lstvConnections.SelectedIndex.ToString(), xmlDocument);
            m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.PreviousOpenNewConnectionEntered), m_connectionAddress.Text, xmlDocument);
            m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.WasOpenedOnExit), m_searchWasRun.ToNumericString(), xmlDocument);

            if (MstscProcesses.IsEnvironmentValidForTracking && cdRunningSection.Width.Value > 0)
            {
                m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.RunningSectionWidth), cdRunningSection.Width.ToString(), xmlDocument);
            }

            Hide();

            m_credentials.Save();
            m_windowGroupedConnections.Save();

            m_settings.SaveDocument(xmlDocument);

            Application.Current.Shutdown();
        }

        private void Window_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            m_windowPosition.Changed = true;
        }

        #endregion

        #region Private "Groups Area" Events

        private void Groups_OnKeyDown(object sender, KeyEventArgs e)
        {
            ListViewAction.FindOnKeydown(e.Key.ToString(), lstvGroups);
        }

        private void Groups_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            ListViewOrdering listViewOrdering = new ListViewOrdering(lstvGroups, e);
            listViewOrdering.Sort();
        }

        private void Groups_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ShowCurrentGroupConnections();
        }

        private void Button_AddNewGroup_OnClick(object sender, RoutedEventArgs e)
        {
            AddGroup addGroup = new AddGroup(m_settings, this)
            {
                Topmost = Topmost,
                Owner = this
            };

            addGroup.ShowDialog();
        }

        private void Button_AddNewConnection_OnClick(object sender, RoutedEventArgs e)
        {
            AddConnection addConnection = new AddConnection(m_settings, this, m_credentials, m_filenameDialog)
            {
                Topmost = Topmost,
                Owner = this
            };

            addConnection.ShowDialog();
        }

        private void MenuItem_Group_OpenAll_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvConnections.Items.Count > 0)
            {
                lstvConnections.SelectedItems.Clear();

                foreach (object connectionItem in lstvConnections.Items)
                {
                    Connection connection = (Connection)connectionItem;

                    if (connection.Enabled)
                    {
                        lstvConnections.SelectedItems.Add(connectionItem);
                    }
                }

                LoadConnections();
            }
        }

        private void MenuItem_Group_MoveUp_OnClick(object sender, RoutedEventArgs e)
        {
            int selectedIndex = lstvGroups.SelectedIndex;
            if (selectedIndex > 0)
            {
                Group group = (Group) lstvGroups.Items[selectedIndex];

                lstvGroups.Items.RemoveAt(selectedIndex);
                lstvGroups.Items.Insert(selectedIndex - 1, group);

                lstvGroups.SelectedIndex = selectedIndex - 1;
            }
        }

        private void MenuItem_Group_MoveDown_OnClick(object sender, RoutedEventArgs e)
        {
            int selectedIndex = lstvGroups.SelectedIndex;
            if (selectedIndex < lstvGroups.Items.Count - 1)
            {
                Group group = (Group) lstvGroups.Items[selectedIndex];

                lstvGroups.Items.RemoveAt(selectedIndex);
                lstvGroups.Items.Insert(selectedIndex + 1, group);

                lstvGroups.SelectedIndex = selectedIndex + 1;
            }
        }

        private void MenuItem_Group_Delete_OnClick(object sender, RoutedEventArgs e)
        {
            int selectedIndex = lstvGroups.SelectedIndex;
            if (selectedIndex > -1)
            {
                Group group = (Group) lstvGroups.Items[selectedIndex];

                bool removeGroup = true;

                if (group.Connections.Count > 0)
                {
                    MessageQuestionResult messageQuestionResult = GetConfirmationFromMessage(Terms.Resources.UIMessages.DeleteGroupConfirmation);
                    removeGroup = messageQuestionResult.Result;
                }

                if (removeGroup)
                {
                    lstvGroups.Items.RemoveAt(selectedIndex);
                    lstvConnections.Items.Clear();

                    if (lstvGroups.Items.Count > 0)
                    {
                        if (selectedIndex > lstvGroups.Items.Count - 1)
                        {
                            lstvGroups.SelectedIndex = selectedIndex - 1;
                        }
                        else
                        {
                            lstvGroups.SelectedIndex = selectedIndex;
                        }
                    }
                }
            }
        }

        private void MenuItem_Group_Edit_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvGroups.SelectedIndex > -1)
            {
                Group group = (Group) lstvGroups.SelectedItem;

                AddGroup addGroup = new AddGroup(m_settings, this, lstvGroups.SelectedIndex, group)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                addGroup.ShowDialog();
            }
        }

        private void ShowCurrentGroupConnections()
        {
            if (lstvGroups.SelectedIndex > -1)
            {
                Group group = (Group) lstvGroups.SelectedItem;

                SetAllowMutipleConnectionManagementMode(group);

                m_windowGroupedConnections.ShowGroupItems(group.Connections);
            }
        }

        private void SetAllowMutipleConnectionManagementMode(Group group)
        {
            lstvConnections.SelectionMode = group.AllowMultipleConnectionManagement 
                ? SelectionMode.Extended 
                : SelectionMode.Single;
        }

        #endregion

        #region Private "Connections Area" Events

        private void Connections_OnKeyDown(object sender, KeyEventArgs e)
        {
            ListViewAction.FindOnKeydown(e.Key.ToString(), lstvConnections);
        }

        private void Connections_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            ListViewOrdering listViewOrdering = new ListViewOrdering(lstvConnections, e);
            listViewOrdering.Sort();
        }
        
        private void Connections_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mnuConnection_Open.IsEnabled)
            {
                LoadConnections();
            }
        }

        private void MenuItem_Connection_Open_OnClick(object sender, RoutedEventArgs e)
        {
            LoadConnections();
        }

        private void MenuItem_Connection_MoveUp_OnClick(object sender, RoutedEventArgs e)
        {
            int selectedGroupIndex = lstvGroups.SelectedIndex;
            int selectedConnectionIndex = lstvConnections.SelectedIndex;

            if (selectedGroupIndex > -1 && selectedConnectionIndex > 0)
            {
                Group group = (Group) lstvGroups.Items[selectedGroupIndex];
                Connection connection = (Connection) lstvConnections.Items[selectedConnectionIndex];

                lstvConnections.Items.RemoveAt(selectedConnectionIndex);
                lstvConnections.Items.Insert(selectedConnectionIndex - 1, connection);

                group.Connections.RemoveAt(selectedConnectionIndex);
                group.Connections.Insert(selectedConnectionIndex - 1, connection);

                lstvConnections.SelectedIndex = selectedConnectionIndex - 1;
            }
        }

        private void MenuItem_Connection_MoveDown_OnClick(object sender, RoutedEventArgs e)
        {
            int selectedGroupIndex = lstvGroups.SelectedIndex;
            int selectedConnectionIndex = lstvConnections.SelectedIndex;

            if (selectedGroupIndex > -1 && selectedConnectionIndex < lstvConnections.Items.Count - 1)
            {
                Group group = (Group) lstvGroups.Items[selectedGroupIndex];
                Connection connection = (Connection) lstvConnections.Items[selectedConnectionIndex];

                lstvConnections.Items.RemoveAt(selectedConnectionIndex);
                lstvConnections.Items.Insert(selectedConnectionIndex + 1, connection);

                group.Connections.RemoveAt(selectedConnectionIndex);
                group.Connections.Insert(selectedConnectionIndex + 1, connection);

                lstvConnections.SelectedIndex = selectedConnectionIndex + 1;
            }
        }

        private void MenuItem_Connection_Copy_Name_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvConnections.SelectedIndex > -1)
            {
                Connection connection = (Connection) lstvConnections.SelectedItem;

                Clipboard.SetText(connection.Name);
            }
        }

        private void MenuItem_Connection_Copy_Address_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvConnections.SelectedIndex > -1)
            {
                Connection connection = (Connection) lstvConnections.SelectedItem;

                Clipboard.SetText(connection.Address);
            }
        }

        private void MenuItem_Connection_Delete_OnClick(object sender, RoutedEventArgs e)
        {
            int[] selectedConnectionIndexes = GetSelectedConnectionIndexes();
            if (selectedConnectionIndexes.Length > 0)
            {
                MessageQuestionResult messageQuestionResult = GetConfirmationFromMessage(Terms.Resources.UIMessages.DeleteSelectedConnectionsConfirmation);

                if (messageQuestionResult.Result)
                {
                    RemoveSelectedConnections(selectedConnectionIndexes);
                }
            }
        }

        private void MenuItem_Connection_DeleteAll_Unused_OnClick(object sender, RoutedEventArgs e)
        {
            MessageQuestionResult messageQuestionResult = GetConfirmationFromMessage(Terms.Resources.UIMessages.DeleteConnectionsConfirmation);

            if (messageQuestionResult.Result)
            {
                int currentListIndex = 0;
                List<int> indexesToRemove = new List<int>();

                foreach (object connectionItem in lstvConnections.Items)
                {
                    Connection connection = (Connection)connectionItem;

                    if (connection.LastAccessed == Terms.Resources.UIMessages.Unknown)
                    {
                        indexesToRemove.Add(currentListIndex);
                    }

                    currentListIndex++;
                }

                RemoveSelectedConnections(indexesToRemove.ToArray());
                ShowMessage(string.Format(Terms.Resources.UIMessages.ConnectionsDeleted, indexesToRemove.Count));
            }
        }

        private void MenuItem_Connection_DeleteAll_Disabled_OnClick(object sender, RoutedEventArgs e)
        {
            MessageQuestionResult messageQuestionResult = GetConfirmationFromMessage(Terms.Resources.UIMessages.DeleteConnectionsConfirmation);

            if (messageQuestionResult.Result)
            {
                int currentListIndex = 0;
                List<int> indexesToRemove = new List<int>();

                foreach (object connectionItem in lstvConnections.Items)
                {
                    Connection connection = (Connection) connectionItem;

                    if (!connection.Enabled)
                    {
                        indexesToRemove.Add(currentListIndex);
                    }

                    currentListIndex++;
                }

                RemoveSelectedConnections(indexesToRemove.ToArray());
                ShowMessage(string.Format(Terms.Resources.UIMessages.ConnectionsDeleted, indexesToRemove.Count));
            }
        }

        private void MenuItem_Connection_DeleteAll_CachedCredentials_OnClick(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                BackgroundAction.Run(() => { mnuConnection_DeleteAll_CachedCredentials.IsEnabled = false; });

                foreach (object connectionItem in lstvConnections.Items)
                {
                    Connection connection = (Connection)connectionItem;

                    Mstsc.DeleteCachedCredentials(connection);
                }

                BackgroundAction.Run(() =>
                {
                    mnuConnection_DeleteAll_CachedCredentials.IsEnabled = true;

                    ShowMessage(string.Format(Terms.Resources.UIMessages.CachedCredentialsDeleted, lstvConnections.Items.Count));
                });
            });
        }

        private void MenuItem_Connection_Move_OnClick(object sender, RoutedEventArgs e)
        {
            int selectedGroupIndex = lstvGroups.SelectedIndex;
            int[] selectedConnectionIndexes = GetSelectedConnectionIndexes();

            if (selectedGroupIndex > -1 && selectedConnectionIndexes.Length > 0)
            {
                Group selectedGroupViewModel = (Group) lstvGroups.SelectedItem;

                MoveConnections moveConnections = new MoveConnections(lstvGroups.Items, selectedGroupViewModel)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                bool? result = moveConnections.ShowDialog();
                if (result != null && result.Value)
                {
                    string groupName = moveConnections.SelectedGroupName;
                    bool copyTheConnections = moveConnections.CopyTheConnections;

                    foreach (object item in lstvGroups.Items)
                    {
                        Group group = (Group) item;

                        if (string.Equals(group.Name, groupName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            foreach (object connectionItem in lstvConnections.SelectedItems)
                            {
                                Connection connection = (Connection) connectionItem;

                                if (copyTheConnections)
                                {
                                    Connection newConnectionViewModel = new Connection();
                                    newConnectionViewModel.Update(connection);
                                    newConnectionViewModel.ResetLastAccessed();
                                    newConnectionViewModel.Name = GetNewConnectionName(group, newConnectionViewModel.Name);

                                    connection = newConnectionViewModel;
                                }
                                else
                                {
                                    connection.Name = GetNewConnectionName(group, connection.Name);
                                }

                                group.Connections.Add(connection);
                            }

                            group.ConnectionsChanged();
                            break;
                        }
                    }

                    if (!copyTheConnections)
                    {
                        RemoveSelectedConnections(selectedConnectionIndexes);
                    }
                }
            }
        }

        private void MenuItem_Connection_Duplicate_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvConnections.SelectedIndex > -1)
            {
                Connection connection = (Connection) lstvConnections.SelectedItem;

                AddConnection addConnection = new AddConnection(m_settings, this, m_credentials, m_filenameDialog, connection: connection)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                addConnection.ShowDialog();
            }
        }

        private void MenuItem_Connection_Ping_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvConnections.SelectedIndex > -1)
            {
                Connection connection = (Connection)lstvConnections.SelectedItem;

                PingConnection pingConnection = new PingConnection(m_settings, m_filenameDialog, connection.Address, connection.Name)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                pingConnection.ShowDialog();

                if (m_updateLastAccessedForPingedConnections)
                {
                    UpdateLastAccessedForConnections();
                }
            }
        }

        private void MenuItem_Connection_Details_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvConnections.SelectedIndex > -1)
            {
                PickCredentials pickCredentials = new PickCredentials(m_credentials)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                bool? result = pickCredentials.ShowDialog();
                if (result != null && result.Value)
                {
                    Connection connection = (Connection)lstvConnections.SelectedItem;

                    ConnectionDetails connectionDetails = new ConnectionDetails(m_settings, connection, pickCredentials.CredentailSelected)
                    {
                        Topmost = Topmost,
                        Owner = this
                    };

                    connectionDetails.ShowDialog();
                }
            }
        }

        private void MenuItem_Connection_Statuses_OnClick(object sender, RoutedEventArgs e)
        {
            int selectedIndex = lstvGroups.SelectedIndex;
            if (selectedIndex > -1)
            {
                Group group = (Group) lstvGroups.Items[selectedIndex];

                List<Connection> connections = lstvConnections.SelectedItems.Count <= 1
                    ? group.Connections.Where(connection => connection.Enabled).ToList()
                    : lstvConnections.SelectedItems.Cast<Connection>().Where(connection => connection.Enabled).ToList();

                ConnectionStatuses connectionStatuses = new ConnectionStatuses(m_settings, connections, this)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                connectionStatuses.ShowDialog();
            }
        }

        private void MenuItem_Connection_ShutdownRestart_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvConnections.SelectedIndex > -1)
            {
                List<Connection> connections = lstvConnections.SelectedItems.Cast<Connection>().Where(connection => connection.Enabled).ToList();

                ShutdownConnection shutdownConnection = new ShutdownConnection(connections, m_settings, m_filenameDialog)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                bool? result = shutdownConnection.ShowDialog();
                if (result != null && result.Value && m_updateLastAccessedWhenConnectionsAreShutdownOrRestarted)
                {
                    UpdateLastAccessedForConnections();
                }
            }
        }

        private void MenuItem_Connection_ServiceManagement_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvConnections.SelectedIndex > -1)
            {
                PickCredentials pickCredentials = new PickCredentials(m_credentials)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                bool? result = pickCredentials.ShowDialog();
                if (result != null && result.Value)
                {
                    List<Connection> connections = lstvConnections.SelectedItems.Cast<Connection>().Where(connection => connection.Enabled).ToList();

                    ConnectionServices connectionDetails = new ConnectionServices(m_settings, connections, pickCredentials.CredentailSelected)
                    {
                        Topmost = Topmost,
                        Owner = this
                    };

                    connectionDetails.ShowDialog();
                }
            }
        }

        private void MenuItem_Connection_SelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            lstvConnections.SelectAll();
        }

        private void MenuItem_Connection_SelectNone_OnClick(object sender, RoutedEventArgs e)
        {
            lstvConnections.SelectedItems.Clear();
        }

        private void MenuItem_Connection_SelectInverse_OnClick(object sender, RoutedEventArgs e)
        {
            ListViewAction.SelectInverse(lstvConnections);
        }

        private void MenuItem_Connection_Edit_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvConnections.SelectedIndex > -1)
            {
                Connection connection = (Connection) lstvConnections.SelectedItem;

                AddConnection addConnection = new AddConnection(m_settings, this, m_credentials, m_filenameDialog, lstvConnections.SelectedIndex, connection)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                addConnection.ShowDialog();
            }
        }

        private int[] GetSelectedConnectionIndexes()
        {
            return (from object selectedItem in lstvConnections.SelectedItems select lstvConnections.Items.IndexOf(selectedItem)).ToArray();
        }

        private void RemoveSelectedConnections(int[] selectedConnectionIndexes)
        {
            Array.Sort(selectedConnectionIndexes, (x, y) => y.CompareTo(x));

            Group group = (Group) lstvGroups.SelectedItem;

            foreach (int selectedIndex in selectedConnectionIndexes)
            {
                lstvConnections.Items.RemoveAt(selectedIndex);

                group.Connections.RemoveAt(selectedIndex);
            }

            group.ConnectionsChanged();
        }

        private void UpdateLastAccessedForConnections()
        {
            foreach (object connectionItem in lstvConnections.SelectedItems)
            {
                Connection connection = (Connection) connectionItem;
                connection.LastAccessed = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void LoadConnections()
        {
            if (lstvConnections.SelectedIndex > -1)
            {
                List<Connection> connections = new List<Connection>();
                List<Connection> connectionViewModelsWithCredentials = new List<Connection>();

                foreach (object connectionItem in lstvConnections.SelectedItems)
                {
                    Connection connection = (Connection) connectionItem;

                    if (!connection.AskForCredentials && !string.IsNullOrEmpty(connection.Username) && !string.IsNullOrEmpty(connection.Username))
                    {
                        connections.Add(connection);
                    }
                    else if (connection.AskForCredentials)
                    {
                        connectionViewModelsWithCredentials.Add(connection);
                    }
                }

                bool connectionsAvailable = connections.Any();
                bool connectionsWithCredentialsAvailable = connectionViewModelsWithCredentials.Any();
                string connectionsUsedUserCredentialName = null;

                if (connectionsAvailable || connectionsWithCredentialsAvailable)
                {
                    bool connectionsOpened = false;

                    if (connectionsWithCredentialsAvailable)
                    {
                        PickCredentials pickCredentials = new PickCredentials(m_mstscProcesses, m_credentials, connectionViewModelsWithCredentials, m_rememberTheLastPickedUserCredentialsForConnections)
                        {
                            Topmost = Topmost,
                            Owner = this
                        };

                        bool? result = pickCredentials.ShowDialog();
                        if (result != null && result.Value)
                        {
                            connectionsOpened = true;

                            if (pickCredentials.CredentailSelected != null)
                            {
                                connectionsUsedUserCredentialName = pickCredentials.CredentailSelected.Name;
                            }
                        }
                    }
                    else
                    {
                        connectionsOpened = true;
                    }

                    if (connectionsWithCredentialsAvailable && connectionsOpened || connectionsOpened)
                    {
                        if (connectionsAvailable)
                        {
                            Mstsc.Open(m_mstscProcesses, connections);
                        }

                        UpdateLastAccessedForConnections();

                        if (!string.IsNullOrEmpty(connectionsUsedUserCredentialName))
                        {
                            foreach (Connection connection in connectionViewModelsWithCredentials)
                            {
                                connection.LastUserCredentialNameUsed = connectionsUsedUserCredentialName;
                            }
                        }

                        if (m_minimizeMainWindowWhenAConnectionIsOpened)
                        {
                            MinimizeWindow();
                        }
                    }
                }
            }
        }

        private static string GetNewConnectionName(Group group, string connectionName)
        {
            string newConnectionName = connectionName;
            int retry = 2;

            while (true)
            {
                bool connectionNameValid = true;

                if (group.Connections.Any(connection => string.Equals(connection.Name, newConnectionName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    newConnectionName = $"{connectionName}_{retry++}";
                    connectionNameValid = false;
                }

                if (connectionNameValid)
                {
                    break;
                }
            }

            return newConnectionName;
        }

        #endregion

        #region Private "Open Connection Area" Events

        private void Button_OpenConnectionArea_Ping_OnClick(object sender, RoutedEventArgs e)
        {
            PingConnection pingConnection = new PingConnection(m_settings, m_filenameDialog, m_connectionAddress.Text)
            {
                Topmost = Topmost,
                Owner = this
            };

            pingConnection.ShowDialog();
        }

        private void Button_OpenConnectionArea_AddConnection_OnClick(object sender, RoutedEventArgs e)
        {
            Connection connection = new Connection
            {
                Address = m_connectionAddress.Text
            };

            AddConnection addConnection = new AddConnection(m_settings, this, m_credentials, m_filenameDialog, connection: connection)
            {
                Topmost = Topmost,
                Owner = this
            };

            addConnection.ShowDialog();
        }

        private void Button_OpenConnectionArea_OpenConnection_OnClick(object sender, RoutedEventArgs e)
        {
            string connectionName = string.Format(Terms.Resources.UIMessages.UntitledConnection, lstvRunning.Items.Count + 1);

            Connection connection = new Connection
            {
                Name = connectionName,
                Address = m_connectionAddress.Text
            };

            List<Connection> connections = new List<Connection>
            {
                connection
            };

            bool connectionOpened;

            if (m_credentials.Count > 0 && m_allowUserCredentialsPickingWhenOpeningConnections)
            {
                PickCredentials pickCredentials = new PickCredentials(m_mstscProcesses, m_credentials, connections, m_rememberTheLastPickedUserCredentialsForConnections)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                bool? result = pickCredentials.ShowDialog();

                connectionOpened = result != null && result.Value;
            }
            else
            {
                Mstsc.Open(m_mstscProcesses, connections, false);

                connectionOpened = true;
            }

            if (connectionOpened && m_minimizeMainWindowWhenAConnectionIsOpened)
            {
                MinimizeWindow();
            }
        }

        private void ConnectionAddressTextChanged()
        {
            string text = m_connectionAddress.Text.ToLower();

            foreach (object connectionItem in lstvConnections.Items)
            {
                if (connectionItem is Connection connection && string.Equals(connection.Address, text, StringComparison.CurrentCultureIgnoreCase))
                {
                    lstvConnections.SelectedItems.Clear();
                    lstvConnections.SelectedItems.Add(connectionItem);

                    ListViewAction.FocusSelectedItem(lstvConnections, lstvConnections.Items.IndexOf(connectionItem));

                    m_connectionAddress.Focus();

                    break;
                }
            }
        }

        #endregion

        #region Private "Running Connections Area" Events

        private void Running_OnKeyDown(object sender, KeyEventArgs e)
        {
            ListViewAction.FindOnKeydown(e.Key.ToString(), lstvRunning);
        }

        private void Running_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            ListViewOrdering listViewOrdering = new ListViewOrdering(lstvRunning, e);
            listViewOrdering.Sort();
        }

        private void Running_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MenuItem_Running_Focus_OnClick(sender, null);
        }

        private void MenuItem_Running_Focus_OnClick(object sender, RoutedEventArgs e)
        {
            if (lstvRunning.SelectedItems.Count == 1)
            {
                SetSelectedProcessesWindowState(NativeMethods.SW_RESTORE, true, m_minimizeAllOtherConnectionsWhenFocusingConnection);
            }
        }

        private void MenuItem_Running_SetAsNormal_OnClick(object sender, RoutedEventArgs e)
        {
            SetSelectedProcessesWindowState(NativeMethods.SW_SHOWNORMAL);
        }

        private void MenuItem_Running_SetAsMinimized_OnClick(object sender, RoutedEventArgs e)
        {
            SetSelectedProcessesWindowState(NativeMethods.SW_SHOWMINIMIZED);
        }

        private void MenuItem_Running_SetAsMaximized_OnClick(object sender, RoutedEventArgs e)
        {
            SetSelectedProcessesWindowState(NativeMethods.SW_SHOWMAXIMIZED);
        }

        private void MenuItem_Running_SelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            lstvRunning.SelectAll();
        }

        private void MenuItem_Running_SelectNone_OnClick(object sender, RoutedEventArgs e)
        {
            lstvRunning.SelectedItems.Clear();
        }

        private void MenuItem_Running_SelectInverse_OnClick(object sender, RoutedEventArgs e)
        {
            ListViewAction.SelectInverse(lstvRunning);
        }

        private void MenuItem_Running_Close_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (object selectedItem in lstvRunning.SelectedItems)
            {
                MstscProcess mstscProcess = (MstscProcess) selectedItem;
                mstscProcess.Process.Kill();
            }
        }

        private void SetSelectedProcessesWindowState(int command, bool setForeground = false, bool minimizeAllOtherConnections = false)
        {
            foreach (object selectedItem in lstvRunning.SelectedItems)
            {
                MstscProcess mstscProcess = (MstscProcess) selectedItem;

                NativeMethods.ShowWindow(mstscProcess.Process.MainWindowHandle, command);

                if (setForeground)
                {
                    NativeMethods.SetForegroundWindow(mstscProcess.Process.MainWindowHandle);
                }
            }

            if (minimizeAllOtherConnections)
            {
                foreach (object item in lstvRunning.Items)
                {
                    if (!lstvRunning.SelectedItems.Contains(item))
                    {
                        MstscProcess mstscProcess = (MstscProcess)item;

                        NativeMethods.ShowWindow(mstscProcess.Process.MainWindowHandle, NativeMethods.SW_SHOWMINIMIZED);
                    }
                }
            }
        }

        #endregion

        #region Private "Message" Helpers

        private MessageQuestionResult GetConfirmationFromMessage(string message, bool showConfirmation = true, string settingSection = null, string settingName = null)
        {
            MessageQuestionResult result = new MessageQuestionResult();

            if (!showConfirmation)
            {
                result.Result = true;
            }

            if (showConfirmation)
            {
                MessageQuestion messageBox = new MessageQuestion(m_settings, message, settingSection, settingName)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                messageBox.ShowDialog();

                result = messageBox.Result;
            }

            return result;
        }

        private void ShowMessage(string message)
        {
            Message messageWindow = new Message(message)
            {
                Topmost = Topmost,
                Owner = this
            };

            messageWindow.ShowDialog();
        }

        #endregion

        #region Private "Tray Icon" Helpers

        private void NotifyTrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Show();
            Activate();
        }

        #endregion
    }
}