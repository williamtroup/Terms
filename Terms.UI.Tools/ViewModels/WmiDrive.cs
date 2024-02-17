using Terms.UI.Tools.ViewModels.Base;

namespace Terms.UI.Tools.ViewModels
{
    public class WmiDrive : Observable, IDataModel
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

        #region Property VolumeName

        private string m_volumeName = string.Empty;

        public string VolumeName
        {
            get => m_volumeName;
            set
            {
                m_volumeName = value;
                OnPropertyChanged(nameof(VolumeName));
            }
        }

        #endregion

        #region Property Size

        private string m_size = string.Empty;

        public string Size
        {
            get => m_size;
            set
            {
                m_size = value;
                OnPropertyChanged(nameof(Size));
            }
        }

        #endregion

        #region Property FreeSpace

        private string m_freespace = string.Empty;

        public string FreeSpace
        {
            get => m_freespace;
            set
            {
                m_freespace = value;
                OnPropertyChanged(nameof(FreeSpace));
            }
        }

        #endregion
    }
}