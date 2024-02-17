using System;
using System.Diagnostics;
using Terms.UI.Tools.ViewModels.Base;

namespace Terms.UI.Tools.Shell
{
    public class MstscProcess : Observable
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

        #region Property Process

        private Process m_process;

        public Process Process
        {
            get => m_process;
            set
            {
                m_process = value;
                OnPropertyChanged(nameof(Process));
            }
        }

        #endregion

        #region Property Started

        private DateTime m_started;

        public DateTime Started
        {
            get => m_started;
            set
            {
                m_started = value;
                OnPropertyChanged(nameof(Started));
            }
        }

        #endregion
    }
}