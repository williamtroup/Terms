using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;
using Terms.Tools.Windows;
using Terms.UI.Tools.Actions;
using Terms.UI.Tools.ViewModels;

namespace Terms.UI.Tools.Shell
{
    public class MstscProcesses : ObservableCollection<MstscProcess>
    {
        #region Private Read-Only Variables

        private readonly ListView m_listView;

        #endregion

        public MstscProcesses(ListView listView)
        {
            m_listView = listView;
        }

        public bool AreProcessesRunning
        {
            get
            {
                bool running;

                if (IsEnvironmentValidForTracking)
                {
                    running = Count > 0;
                }
                else
                {
                    running = Processes.IsRunning(Mstsc.ConnectionExecutableName);
                }

                return running;
            }
        }

        public MstscProcess Add(Connection connection, Process process)
        {
            MstscProcess mstscProcess = null;

            if (IsEnvironmentValidForTracking)
            {
                string name = !string.IsNullOrEmpty(connection.Username)
                    ? $"{connection.Name} [{connection.Username}]"
                    : connection.Name;

                mstscProcess = new MstscProcess
                {
                    Name = name,
                    Process = process,
                    Started = DateTime.Now
                };

                Add(mstscProcess);

                BackgroundAction.Run(() =>
                {
                    m_listView.Items.Add(mstscProcess);
                });
            }

            return mstscProcess;
        }

        public new void Remove(MstscProcess mstscProcess)
        {
            base.Remove(mstscProcess);

            BackgroundAction.Run(() =>
            {
                m_listView.Items.Remove(mstscProcess);
            });
        }

        public static bool IsEnvironmentValidForTracking => (IntPtr.Size == 8 && Environment.Is64BitOperatingSystem) ||
                                                            (IntPtr.Size == 4 && !Environment.Is64BitOperatingSystem);
    }
}