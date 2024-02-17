using Terms.UI.Tools.ViewModels.Base;

namespace Terms.UI.Tools.ViewModels
{
    public class WmiDetail : Observable, IDataModel
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

        #region Property Value

        private string m_value = string.Empty;

        public string Value
        {
            get => m_value;
            set
            {
                m_value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        #endregion
    }
}