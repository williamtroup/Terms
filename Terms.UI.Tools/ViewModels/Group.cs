using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Terms.UI.Tools.ViewModels.Base;

namespace Terms.UI.Tools.ViewModels;

[Serializable]
public class Group : Observable, IDataModel
{
    public Group()
    {
        Connections = new List<Connection>();
    }

    private string m_name = string.Empty;

    public string Name
    {
        get => m_name;
        set
        {
            m_name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    private List<Connection> m_connections;

    public List<Connection> Connections
    {
        get => m_connections;
        private set
        {
            m_connections = value;
            ConnectionsChanged();
        }
    }

    private Visibility m_visibility = Visibility.Visible;

    [XmlIgnore]
    public Visibility Visibility
    {
        get => m_visibility;
        set
        {
            m_visibility = value;
            OnPropertyChanged(nameof(Visibility));
        }
    }

    [XmlIgnore]
    public int TotalConnections
    {
        get
        {
            return Connections.Count(connection => connection.Visibility == Visibility.Visible);
        }
    }

    private string m_notes;

    public string Notes
    {
        get => m_notes;
        set
        {
            m_notes = string.IsNullOrEmpty(value) ? null : value;
            OnPropertyChanged(nameof(Notes));
        }
    }

    private bool m_allowMultipleConnectionManagement = true;

    public bool AllowMultipleConnectionManagement
    {
        get => m_allowMultipleConnectionManagement;
        set
        {
            m_allowMultipleConnectionManagement = value;
            OnPropertyChanged(nameof(AllowMultipleConnectionManagement));
        }
    }

    private bool m_allowAllPasswordsToBeChanged;

    public bool AllowAllPasswordsToBeChanged
    {
        get => m_allowAllPasswordsToBeChanged;
        set
        {
            m_allowAllPasswordsToBeChanged = value;
            OnPropertyChanged(nameof(AllowAllPasswordsToBeChanged));
        }
    }

    [XmlIgnore]
    public SolidColorBrush ForeColor
    {
        get
        {
            SolidColorBrush foreColor = Brushes.Black;

            if (Connections == null || Connections.Count == 0)
            {
                foreColor = Brushes.Gray;
            }

            return foreColor;
        }
    }

    [XmlIgnore]
    public FontStyle FontStyle
    {
        get
        {
            FontStyle fontStyle = FontStyles.Normal;

            if (Connections == null || Connections.Count == 0)
            {
                fontStyle = FontStyles.Italic;
            }

            return fontStyle;
        }
    }

    public void Update(Group model)
    {
        Name = model.Name;
        Notes = model.Notes;
        AllowMultipleConnectionManagement = model.AllowMultipleConnectionManagement;
        AllowAllPasswordsToBeChanged = model.AllowAllPasswordsToBeChanged;
    }

    public void ConnectionsChanged()
    {
        OnPropertyChanged(nameof(Connections));
        OnPropertyChanged(nameof(TotalConnections));
        OnPropertyChanged(nameof(Notes));
        OnPropertyChanged(nameof(AllowMultipleConnectionManagement));
        OnPropertyChanged(nameof(AllowAllPasswordsToBeChanged));
        OnPropertyChanged(nameof(ForeColor));
        OnPropertyChanged(nameof(FontStyle));
    }
}