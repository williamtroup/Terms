using Microsoft.Win32;
using System.IO;

namespace Terms.UI.Tools.Views
{
    public class FilenameDialog
    {
        #region Private Read-Only Variables

        private readonly WindowGroupedConnections m_windowGroupedConnections;

        #endregion

        public FilenameDialog(WindowGroupedConnections groupedConnections)
        {
            m_windowGroupedConnections = groupedConnections;
        }

        public void Open(string filter, string title)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title
            };

            bool? result = openFileDialog.ShowDialog();
            if (result != null && result.Value)
            {
                m_windowGroupedConnections.Load(openFileDialog.FileName);
            }
        }

        public bool Open(string filter, string title, ref string filename)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title
            };

            bool? result = openFileDialog.ShowDialog();
            if (result != null && result.Value)
            {
                filename = openFileDialog.FileName;
            }

            return result != null && result.Value;
        }

        public void Save(string filter, string title)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                Title = title
            };

            bool? result = saveFileDialog.ShowDialog();
            if (result != null && result.Value)
            {
                if (File.Exists(saveFileDialog.FileName))
                {
                    File.Delete(saveFileDialog.FileName);
                }

                m_windowGroupedConnections.Save(saveFileDialog.FileName);
            }
        }

        public bool Save(string filter, string title, ref string filename)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                Title = title
            };

            bool? result = saveFileDialog.ShowDialog();
            if (result != null && result.Value)
            {
                filename = saveFileDialog.FileName;
            }

            return result != null && result.Value;
        }
    }
}