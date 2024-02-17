using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Terms.UI.Tools.Actions;

public class ListViewOrdering(ListView listview, RoutedEventArgs routedEventArgs)
{
    private readonly ListView m_listview = listview;
    private readonly RoutedEventArgs m_routedEventArgs = routedEventArgs;

    public void Sort()
    {
        int columnIndex = GetColumnIndex();
        if (columnIndex > -1 && m_listview.View is GridView gridview && gridview.Columns[columnIndex].DisplayMemberBinding is Binding binding)
        {
            SortByColumnBindingName(binding.Path.Path);
        }
    }

    private int GetColumnIndex()
    {
        int columnIndex = -1;

        if (m_routedEventArgs.OriginalSource is GridViewColumnHeader header && header.Parent is GridViewHeaderRowPresenter rowPresenter)
        {
            columnIndex = rowPresenter.Columns.IndexOf(header.Column);
        }

        return columnIndex;
    }

    private void SortByColumnBindingName(string bindingName)
    {
        if (!string.IsNullOrEmpty(bindingName))
        {
            ListSortDirection listSortDirection = ListSortDirection.Ascending;

            if (m_listview.Items.SortDescriptions.Count > 0)
            {
                listSortDirection = m_listview.Items.SortDescriptions[0].Direction == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;

                m_listview.Items.SortDescriptions.Clear();
            }

            m_listview.Items.SortDescriptions.Add(new SortDescription(bindingName, listSortDirection));
        }
    }
}