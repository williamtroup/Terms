using Terms.UI.Tools.ViewModels.Base;

namespace Terms.UI.Tools.ViewModels
{
    public class WmiProcess : Observable, IDataModel
    {
        #region Property Id

        private string m_id = string.Empty;

        public string Id
        {
            get => m_id;
            set
            {
                m_id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        #endregion

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
    }
}