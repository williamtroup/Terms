using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Terms.UI.Tools.Actions;
using Terms.UI.Tools.Extensions;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.Views;

namespace Terms.Windows.Management
{
    public partial class MoveConnections
    {
        #region Private Read-Only Variables

        private readonly ItemCollection m_itemCollection;
        private readonly Group m_selectedGroupViewModel;

        #endregion

        public MoveConnections(ItemCollection itemCollection, Group selectedGroupViewModel)
        {
            InitializeComponent();

            WindowLayout.Setup(this, WindowBorder);

            m_itemCollection = itemCollection;
            m_selectedGroupViewModel = selectedGroupViewModel;

            SetupDisplay();
        }

        private void SetupDisplay()
        {
            foreach (object item in m_itemCollection)
            {
                Group group = (Group) item;

                if (!string.Equals(group.Name, m_selectedGroupViewModel.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    ListBoxItem listBoxItem = new ListBoxItem
                    {
                        Content = group.Name,
                        ToolTip = group.Notes
                    };

                    lstGroups.Items.Add(listBoxItem);
                }
            }
        }

        #region Public Properties

        public string SelectedGroupName { get; set; }
        public bool CopyTheConnections { get; set; }

        #endregion

        #region Private "Pick" Events

        private void Groups_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Button_OK_Click(sender, null);
            }
        }

        private void Groups_OnKeyDown(object sender, KeyEventArgs e)
        {
            ListAction.FindOnKeydown(e.Key.ToString(), lstGroups);
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectedItem = (ListBoxItem) lstGroups.Items[lstGroups.SelectedIndex];

            SelectedGroupName = selectedItem.Content.ToString();
            CopyTheConnections = chkCopyTheConnections.IsReallyChecked();

            DialogResult = true;
        }

        #endregion
    }
}