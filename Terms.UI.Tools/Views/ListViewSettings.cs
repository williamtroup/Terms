using System.Globalization;
using System.Windows.Controls;
using System.Xml;
using Terms.Tools.Settings.Interfaces;

namespace Terms.UI.Tools.Views
{
    public class ListViewSettings
    {
        private readonly IXmlSettings m_settings;
        private readonly GridView m_gridView;
        private readonly string m_sectionName;

        public ListViewSettings(IXmlSettings settings, ListView listView, string sectionName = "Columns", XmlDocument xmlDocument = null)
        {
            m_settings = settings;
            m_gridView = (GridView) listView.View;
            m_sectionName = !sectionName.Contains("Columns") ? $"{sectionName}Columns" : sectionName;

            GetColumnWidths(xmlDocument);
        }

        public void SetColumnWidths(XmlDocument xmlDocument = null)
        {
            int columnIndex = 0;

            XmlDocument actualXmlDocument = xmlDocument ?? m_settings.GetDocument();

            foreach (GridViewColumn column in m_gridView.Columns)
            {
                m_settings.Write(m_sectionName, columnIndex.ToString(), column.Width.ToString(CultureInfo.InvariantCulture), actualXmlDocument);
                columnIndex++;
            }

            if (xmlDocument == null)
            {
                m_settings.SaveDocument(actualXmlDocument);
            }
        }

        private void GetColumnWidths(XmlDocument xmlDocument)
        {
            int columnIndex = 0;

            XmlDocument actualXmlDocument = xmlDocument ?? m_settings.GetDocument();

            foreach (GridViewColumn column in m_gridView.Columns)
            {
                string width = m_settings.Read(m_sectionName, columnIndex.ToString(), "0", actualXmlDocument);

                if (double.TryParse(width, out double newWidth) && newWidth > 0)
                {
                    column.Width = newWidth;
                }

                columnIndex++;
            }
        }
    }
}