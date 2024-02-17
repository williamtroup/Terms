using System.Windows;
using Terms.UI.Tools.ViewModels.Base;

namespace Terms.UI.Tools.ViewModels
{
    public class WmiService : Observable, IDataModel
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

        private string m_description = string.Empty;

        public string Description
        {
            get => m_description;
            set
            {
                m_description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private string m_state = string.Empty;

        public string State
        {
            get => m_state;
            set
            {
                m_state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        private Visibility m_visibility = Visibility.Visible;

        public Visibility Visibility
        {
            get => m_visibility;
            set
            {
                m_visibility = value;
                OnPropertyChanged(nameof(Visibility));
            }
        }
    }
}