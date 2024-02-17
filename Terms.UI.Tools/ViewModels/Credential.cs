using System;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Terms.UI.Tools.ViewModels.Base;

namespace Terms.UI.Tools.ViewModels;

[Serializable]
public class Credential : Observable, IDataModel
{
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

    private string m_address = string.Empty;

    public string Address
    {
        get => m_address;
        set
        {
            m_address = value;
            OnPropertyChanged(nameof(Address));
        }
    }

    private string m_username = string.Empty;

    public string Username
    {
        get => m_username;
        set
        {
            m_username = value;
            OnPropertyChanged(nameof(Username));
        }
    }

    private string m_password = string.Empty;

    public string Password
    {
        get => m_password;
        set
        {
            m_password = value;
            OnPropertyChanged(nameof(Password));
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

    private bool m_enabled = true;

    public bool Enabled
    {
        get => m_enabled;
        set
        {
            m_enabled = value;
            OnPropertyChanged(nameof(Enabled));
            OnPropertyChanged(nameof(EnabledYesNo));
            OnPropertyChanged(nameof(ForeColor));
            OnPropertyChanged(nameof(FontStyle));
        }
    }

    [XmlIgnore]
    public string EnabledYesNo => m_enabled ? "Yes" : "No";

    [XmlIgnore]
    public SolidColorBrush ForeColor
    {
        get
        {
            SolidColorBrush foreColor = Brushes.Black;

            if (!Enabled)
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

            if (!Enabled)
            {
                fontStyle = FontStyles.Italic;
            }

            return fontStyle;
        }
    }

    public void Update(Credential model)
    {
        Name = model.Name;
        Address = model.Address;
        Username = model.Username;
        Password = model.Password;
        Notes = model.Notes;
        Enabled = model.Enabled;
    }
}