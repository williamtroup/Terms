using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Terms.Tools.Extensions;
using Terms.Tools.Settings.Interfaces;
using Terms.UI.Tools;
using Terms.UI.Tools.Actions;
using Terms.UI.Tools.Enums;
using Terms.UI.Tools.Extensions;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.Views;

namespace Terms.Windows.List
{
    public partial class Search
    {
        #region Private Read-Only Variables

        private readonly IXmlSettings m_settings;
        private readonly WindowPosition m_windowPosition;
        private readonly ListView m_groups;
        private readonly ListView m_connections;
        private readonly Main m_main;

        #endregion

        #region Private Variables

        private bool m_wasSearchConducted;
        private int m_lastGroupIndex;
        private int m_lastConnectionIndex;
        private bool m_allItemsVisibielStateSet;

        #endregion

        public Search(IXmlSettings settings, ListView groups, ListView connections, Main main)
        {
            InitializeComponent();
            
            m_settings = settings;
            m_groups = groups;
            m_connections = connections;
            m_main = main;
            m_windowPosition = new WindowPosition(this, m_settings, Width, Height, GetName);

            DoFullSaveofSettings = true;

            SetupDisplay();

            BackgroundAction.Run(() => m_windowPosition.Get());
        }

        private static string GetName => string.Format(Settings.WindowNameFormat, nameof(Search), Settings.Window);

        private void SetupDisplay()
        {
            XmlDocument xmlDocument = m_settings.GetDocument();

            int showOptions = Convert.ToInt32(m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.ShowOptions), Settings.SearchWindow.ShowOptions, xmlDocument));
            int matchCase = Convert.ToInt32(m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.MatchCase), Settings.SearchWindow.MatchCase, xmlDocument));
            int matchOnlyUsedConnections = Convert.ToInt32(m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.MatchOnlyUsedConnections), Settings.SearchWindow.MatchOnlyUsedConnections, xmlDocument));
            int makeTransparentWhenFocusIsLost = Convert.ToInt32(m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.MakeTransparentWhenFocusIsLost), Settings.SearchWindow.MakeTransparentWhenFocusIsLost, xmlDocument));
            string searchType = m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.SearchType), Settings.SearchWindow.SearchType, xmlDocument);
            string lastSearch = m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.LastSearch), Settings.SearchWindow.LastSearch, xmlDocument);
            int chowAllMatchingItems = Convert.ToInt32(m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.ShowAllMatchingItems), Settings.SearchWindow.ShowAllMatchingItems, xmlDocument));
            string searchArea = m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.SearchArea), Settings.SearchWindow.SearchArea, xmlDocument);

            SearchType actualSearchType = !string.IsNullOrEmpty(searchType) ? (SearchType)Enum.Parse(typeof(SearchType), searchType) : SearchType.Contains;
            SearchArea actualSearchArea = !string.IsNullOrEmpty(searchArea) ? (SearchArea)Enum.Parse(typeof(SearchArea), searchArea) : SearchArea.Both;

            spSearchOptions.Visibility = showOptions > 0 ? Visibility.Visible : Visibility.Collapsed;
            chkShowOptions.IsChecked = showOptions > 0;
            chkMatchCase.IsChecked = matchCase > 0;
            chkMatchOnlyUsedConnections.IsChecked = matchOnlyUsedConnections > 0;
            chkMakeTransparentWhenFocusIsLost.IsChecked = makeTransparentWhenFocusIsLost > 0;
            txtFind.Text = lastSearch;
            chkShowAllMatchingItems.IsChecked = chowAllMatchingItems > 0;

            SetSearchType(actualSearchType);
            SetSearchArea(actualSearchArea);

            lblMessage.Visibility = Visibility.Hidden;

            txtFind.Focus();

            if (!string.IsNullOrEmpty(lastSearch))
            {
                txtFind.SelectAll();
            }
        }

        private void SetSearchType(SearchType actualSearchType)
        {
            switch (actualSearchType)
            {
                case SearchType.StartsWith:
                    opStartsWith.IsChecked = true;
                    break;

                case SearchType.EndsWith:
                    opEndsWith.IsChecked = true;
                    break;

                case SearchType.WholeWordOnly:
                    opWholeWordOnly.IsChecked = true;
                    break;

                default:
                    opContains.IsChecked = true;
                    break;
            }
        }

        private void SetSearchArea(SearchArea actualSearchArea)
        {
            switch (actualSearchArea)
            {
                case SearchArea.Groups:
                    opSearchGroups.IsChecked = true;
                    break;

                case SearchArea.Connections:
                    opSearchConnections.IsChecked = true;
                    break;

                default:
                    opSearchBoth.IsChecked = true;
                    break;
            }
        }

        public void ShowWindow(bool runSearch = false)
        {
            Show();

            if (runSearch && bFind.IsEnabled)
            {
                Button_Find_Click(null, null);
            }
        }

        public bool DoFullSaveofSettings { get; set; }

        #region Private "Window Title Bar" Events

        private void Title_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();

                m_windowPosition.Changed = true;
            }
        }

        #endregion

        #region Private "Window" Events

        private void Window_OnActivated(object sender, EventArgs e)
        {
            WindowBorder.BorderBrush = WindowLayout.BorderActivatedColor;
            lblTitle.Background = WindowLayout.BorderActivatedColor;

            if (chkMakeTransparentWhenFocusIsLost.IsReallyChecked())
            {
                Opacity = 1.0;
            }
        }

        private void Window_OnDeactivated(object sender, EventArgs e)
        {
            WindowBorder.BorderBrush = WindowLayout.BorderDeactivatedColor;
            lblTitle.Background = WindowLayout.BorderDeactivatedColor;

            if (chkMakeTransparentWhenFocusIsLost.IsReallyChecked())
            {
                Opacity = 0.5;
            }
        }

        private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (KeyStroke.IsAltKey(Key.Space))
            {
                e.Handled = true;
            }
        }

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_windowPosition.Set();

            XmlDocument xmlDocument = m_settings.GetDocument();

            m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.ShowOptions), chkShowOptions.IsReallyChecked().ToNumericString(), xmlDocument);

            if (m_wasSearchConducted)
            {
                m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.MatchCase), chkMatchCase.IsReallyChecked().ToNumericString(), xmlDocument);
                m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.MatchOnlyUsedConnections), chkMatchOnlyUsedConnections.IsReallyChecked().ToNumericString(), xmlDocument);
                m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.MakeTransparentWhenFocusIsLost), chkMakeTransparentWhenFocusIsLost.IsReallyChecked().ToNumericString(), xmlDocument);
                m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.SearchType), GetSearchType.ToString(), xmlDocument);
                m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.LastSearch), txtFind.Text, xmlDocument);
                m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.ShowAllMatchingItems), chkShowAllMatchingItems.IsReallyChecked().ToNumericString(), xmlDocument);
                m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.SearchArea), GetSearchArea.ToString(), xmlDocument);
            }

            m_settings.SaveDocument(xmlDocument);

            if (DoFullSaveofSettings)
            {
                if (m_allItemsVisibielStateSet)
                {
                    SetVisibilityForAllEntries(Visibility.Visible);
                }

                m_main.ClearSearchUsage();
            }
        }

        #endregion

        #region Private "Button" Events

        private void Button_Find_Click(object sender, RoutedEventArgs e)
        {
            if (chkShowAllMatchingItems.IsReallyChecked())
            {
                SetVisibilityForAllEntries(chkShowAllMatchingItems.IsReallyChecked() ? Visibility.Collapsed : Visibility.Visible);
            }
            else
            {
                if (m_allItemsVisibielStateSet)
                {
                    SetVisibilityForAllEntries(Visibility.Visible);
                }
            }

            if (RunSearch())
            {
                lblMessage.Visibility = Visibility.Hidden;
                m_main.SetSearchAsRun();
            }
            else
            {
                lblMessage.Visibility = Visibility.Visible;
                lblMessage.Content = Terms.Resources.UIMessages.NoResultsFound;

                if (chkShowAllMatchingItems.IsReallyChecked() && m_allItemsVisibielStateSet)
                {
                    SetVisibilityForAllEntries(Visibility.Visible);
                }
            }
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            if (m_allItemsVisibielStateSet)
            {
                SetVisibilityForAllEntries(Visibility.Visible);
            }

            m_allItemsVisibielStateSet = false;

            txtFind.Text = string.Empty;
            txtFind.Focus();

            bClear.IsEnabled = false;
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Private "Search" Helpers

        private bool RunSearch(bool searchAgain = true)
        {
            bool matchCase = chkMatchCase.IsReallyChecked();
            SearchType searchType = GetSearchType;
            SearchArea searchArea = GetSearchArea;
            string find = txtFind.Text;

            int groupListIndex = m_lastGroupIndex > 0 ? m_lastGroupIndex + 1 : m_lastGroupIndex;
            int connectionListIndex = m_lastConnectionIndex > 0 ? m_lastConnectionIndex + 1 : m_lastConnectionIndex;

            if (chkShowAllMatchingItems.IsReallyChecked())
            {
                groupListIndex = 0;
                connectionListIndex = 0;
            }

            bool found = false;

            for (; groupListIndex < m_groups.Items.Count; groupListIndex++)
            {
                UI.Tools.ViewModels.Group group = (UI.Tools.ViewModels.Group)m_groups.Items[groupListIndex];

                if (searchArea == SearchArea.Both || searchArea == SearchArea.Groups)
                {
                    if (SearchDataForText(group.Name, find, matchCase, searchType))
                    {
                        if (chkShowAllMatchingItems.IsReallyChecked())
                        {
                            group.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            m_groups.SelectedIndex = groupListIndex;
                            m_groups.UpdateLayout();

                            ListViewAction.FocusSelectedItem(m_groups, groupListIndex);
                        }

                        found = true;
                    }
                }

                if (searchArea == SearchArea.Both || searchArea == SearchArea.Connections)
                {
                    if (!found || chkShowAllMatchingItems.IsReallyChecked())
                    {
                        for (; connectionListIndex < group.Connections.Count; connectionListIndex++)
                        {
                            Connection connection = group.Connections[connectionListIndex];

                            if ((SearchDataForText(connection.Name, find, matchCase, searchType) ||
                                 SearchDataForText(connection.Address, find, matchCase, searchType)) &&
                                IsLastUpdatedValid(connection))
                            {
                                if (chkShowAllMatchingItems.IsReallyChecked())
                                {
                                    group.Visibility = Visibility.Visible;
                                    connection.Visibility = Visibility.Visible;

                                    group.ConnectionsChanged();
                                }
                                else
                                {
                                    m_groups.SelectedIndex = groupListIndex;
                                    m_groups.UpdateLayout();

                                    m_connections.SelectedIndex = connectionListIndex;
                                    m_connections.UpdateLayout();

                                    ListViewAction.FocusSelectedItem(m_groups, groupListIndex);
                                    ListViewAction.FocusSelectedItem(m_connections, connectionListIndex);
                                }

                                found = true;

                                if (!chkShowAllMatchingItems.IsReallyChecked())
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                if (found && !chkShowAllMatchingItems.IsReallyChecked())
                {
                    break;
                }
                else
                {
                    connectionListIndex = 0;
                }
            }

            if (!found && searchAgain && !chkShowAllMatchingItems.IsReallyChecked())
            {
                m_lastGroupIndex = 0;
                m_lastConnectionIndex = 0;

                found = RunSearch(false);
            }
            else
            {
                if (found)
                {
                    if (chkShowAllMatchingItems.IsReallyChecked())
                    {
                        HideGroupsThatShowNoConnections();

                        if (GetTotalVisibleGroups() > 0)
                        {
                            SelectFirstVisibleGroupAndConnectionInResults();
                        }
                        else
                        {
                            found = false;
                        }
                    }
                    else
                    {
                        m_lastGroupIndex = groupListIndex;
                        m_lastConnectionIndex = connectionListIndex;
                    }
                }
            }

            m_wasSearchConducted = true;

            return found;
        }

        private static bool SearchDataForText(string searchText, string textToFind, bool matchCase, SearchType searchType)
        {
            bool returnFlag = false;

            string entrySearchText = matchCase ? searchText : searchText.ToLower();
            string entryTextToFind = matchCase ? textToFind : textToFind.ToLower();

            switch (searchType)
            {
                case SearchType.Contains:
                    returnFlag = entrySearchText.Contains(entryTextToFind);
                    break;

                case SearchType.StartsWith:
                    returnFlag = entrySearchText.StartsWith(entryTextToFind);
                    break;

                case SearchType.EndsWith:
                    returnFlag = entrySearchText.EndsWith(entryTextToFind);
                    break;

                case SearchType.WholeWordOnly:

                    string regEx = $"\\b{entryTextToFind}\\b";

                    returnFlag = Regex.IsMatch(entrySearchText, regEx);
                    break;
            }

            return returnFlag;
        }

        private bool IsLastUpdatedValid(Connection connection)
        {
            return !chkMatchOnlyUsedConnections.IsReallyChecked() || !string.IsNullOrEmpty(connection.LastAccessed) && connection.LastAccessed != Terms.Resources.UIMessages.Unknown;
        }

        private SearchType GetSearchType
        {
            get
            {
                SearchType searchType = SearchType.Contains;

                if (opStartsWith.IsReallyChecked())
                {
                    searchType = SearchType.StartsWith;
                }
                else if (opEndsWith.IsReallyChecked())
                {
                    searchType = SearchType.EndsWith;
                }
                else if (opWholeWordOnly.IsReallyChecked())
                {
                    searchType = SearchType.WholeWordOnly;
                }

                return searchType;
            }
        }

        private SearchArea GetSearchArea
        {
            get
            {
                SearchArea searchArea = SearchArea.Both;

                if (opSearchGroups.IsReallyChecked())
                {
                    searchArea = SearchArea.Groups;
                }
                else if (opSearchConnections.IsReallyChecked())
                {
                    searchArea = SearchArea.Connections;
                }

                return searchArea;
            }
        }

        private void SetVisibilityForAllEntries(Visibility visibility)
        {
            foreach (object groupItem in m_groups.Items)
            {
                UI.Tools.ViewModels.Group group = (UI.Tools.ViewModels.Group) groupItem;
                group.Visibility = visibility;

                foreach (Connection connection in group.Connections)
                {
                    connection.Visibility = visibility;
                }

                group.ConnectionsChanged();
            }

            m_allItemsVisibielStateSet = true;
            bClear.IsEnabled = true;
        }

        private void HideGroupsThatShowNoConnections()
        {
            foreach (object groupItem in m_groups.Items)
            {
                UI.Tools.ViewModels.Group group = (UI.Tools.ViewModels.Group) groupItem;

                if (group.TotalConnections <= 0)
                {
                    group.Visibility = Visibility.Collapsed;
                }
            }
        }

        private int GetTotalVisibleGroups()
        {
            return m_groups.Items.Cast<UI.Tools.ViewModels.Group>().Count(group => group.Visibility == Visibility.Visible);
        }

        private void SelectFirstVisibleGroupAndConnectionInResults()
        {
            for (int groupListIndex = 0; groupListIndex < m_groups.Items.Count; groupListIndex++)
            {
                UI.Tools.ViewModels.Group group = (UI.Tools.ViewModels.Group)m_groups.Items[groupListIndex];

                if (group.Visibility == Visibility.Visible)
                {
                    m_groups.SelectedIndex = groupListIndex;
                    m_groups.UpdateLayout();

                    for (int connectionListIndex = 0; connectionListIndex < group.Connections.Count; connectionListIndex++)
                    {
                        Connection connection = group.Connections[connectionListIndex];

                        if (connection.Visibility == Visibility.Visible)
                        {
                            m_connections.SelectedIndex = connectionListIndex;
                            m_connections.UpdateLayout();

                            break;
                        }
                    }

                    break;
                }
            }
        }


        #endregion

        #region Private "CheckBox" Events

        private void CheckBox_ShowOptions_CheckedChanged(object sender, RoutedEventArgs e)
        {
            spSearchOptions.Visibility = chkShowOptions.IsReallyChecked() ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion
    }
}