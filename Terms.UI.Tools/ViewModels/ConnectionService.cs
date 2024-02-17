using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Terms.UI.Tools.ViewModels.Base;

namespace Terms.UI.Tools.ViewModels
{
    public class ConnectionService : Observable, IDataModel
    {
        #region Property Name

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

        #endregion

        #region Property Address

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

        #endregion

        #region Property Status

        private string m_status = string.Empty;

        public string Status
        {
            get => m_status;
            set
            {
                m_status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        #endregion

        #region Property DateTime

        private string m_dateTime = System.DateTime.Now.ToString(CultureInfo.InvariantCulture);

        public string DateTime
        {
            get => m_dateTime;
            set
            {
                m_dateTime = value;
                OnPropertyChanged(nameof(DateTime));
            }
        }

        #endregion

        #region Property Visibility

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

        #endregion

        #region Property ForeColor

        private SolidColorBrush m_foreColor = Brushes.Black;

        public SolidColorBrush ForeColor
        {
            get => m_foreColor;
            set
            {
                m_foreColor = value;
                OnPropertyChanged(nameof(ForeColor));
            }
        }

        #endregion

        #region Property FontStyle

        private FontStyle m_fontStyle = FontStyles.Normal;

        public FontStyle FontStyle
        {
            get => m_fontStyle;
            set
            {
                m_fontStyle = value;
                OnPropertyChanged(nameof(FontStyle));
            }
        }

        #endregion

        #region Property Completed

        private bool m_completed;

        public bool Completed
        {
            get => m_completed;
            set
            {
                m_completed = value;
                OnPropertyChanged(nameof(Completed));
            }
        }

        #endregion
    }
}