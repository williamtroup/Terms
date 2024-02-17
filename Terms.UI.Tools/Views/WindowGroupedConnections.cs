using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Terms.UI.Tools.ViewModels;
using Terms.UI.Tools.ViewModels.Storage;

namespace Terms.UI.Tools.Views;

public class WindowGroupedConnections(ListView groupList, ListView connectionsList)
{
    private readonly ListView m_groupList = groupList;
    private readonly ListView m_connectionsList = connectionsList;

    public void Load(string filename = Connections.DefaultFilename, bool clearItems = true, Action afterLoadingAction = null)
    {
        Connections connections = new();
        connections.Load(filename);

        if (clearItems)
        {
            m_groupList.Items.Clear();
            m_connectionsList.Items.Clear();
        }

        foreach (Group group in connections.Groups)
        {
            m_groupList.Items.Add(group);
        }

        m_groupList.UpdateLayout();

        afterLoadingAction?.Invoke();
    }

    public void Save(string filename = Connections.DefaultFilename)
    {
        Connections connections = new();

        foreach (object item in m_groupList.Items)
        {
            Group group = (Group) item;

            connections.Groups.Add(group);
        }

        connections.Save(filename);
    }

    public void ShowGroupItems(IEnumerable<Connection> connections)
    {
        if (m_connectionsList.Items.Count > 0)
        {
            m_connectionsList.Items.Clear();
        }

        foreach (Connection connection in connections)
        {
            m_connectionsList.Items.Add(connection);
        }

        m_connectionsList.UpdateLayout();
    }
}