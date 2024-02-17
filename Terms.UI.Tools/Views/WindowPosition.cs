using System;
using System.Globalization;
using System.Windows;
using System.Xml;
using Terms.Tools.Settings.Interfaces;

namespace Terms.UI.Tools.Views
{
    public class WindowPosition(
        Window window,
        IXmlSettings settings,
        double defaultWidth,
        double defaultHeight,
        string sectionName = "Window")
    {
        #region Private Read-Only Variables

        private readonly Window m_window = window;
        private readonly IXmlSettings m_settings = settings;
        private readonly double m_defaultWidth = defaultWidth;
        private readonly double m_defaultHeight = defaultHeight;
        private readonly string m_sectionName = sectionName;

        #endregion

        public void Get(bool ignoreWindowResizeMode = false, XmlDocument xmlDocument = null)
        {
            XmlDocument actualXmlDocument = xmlDocument ?? m_settings.GetDocument();

            double x = Convert.ToDouble(m_settings.Read(m_sectionName, "X", "0", actualXmlDocument));
            double y = Convert.ToDouble(m_settings.Read(m_sectionName, "Y", "0", actualXmlDocument));
            int minimized = Convert.ToInt32(m_settings.Read(m_sectionName, "Minimized", "0", actualXmlDocument));
            int maximized = Convert.ToInt32(m_settings.Read(m_sectionName, "Maximized", "0", actualXmlDocument));
            int hidden = Convert.ToInt32(m_settings.Read(m_sectionName, "Hidden", "0", actualXmlDocument));
            int saved = Convert.ToInt32(m_settings.Read(m_sectionName, "Saved", "0", actualXmlDocument));

            if (saved > 0)
            {
                double width = Convert.ToDouble(m_settings.Read(m_sectionName, "Width", m_defaultWidth.ToString(CultureInfo.InvariantCulture), actualXmlDocument));
                double height = Convert.ToDouble(m_settings.Read(m_sectionName, "Height", m_defaultHeight.ToString(CultureInfo.InvariantCulture), actualXmlDocument));

                if (minimized > 0 || maximized > 0)
                {
                    m_window.Left = x;
                    m_window.Top = y;
                    m_window.Width = width;
                    m_window.Height = height;
                }

                if (minimized > 0)
                {
                    m_window.WindowState = WindowState.Minimized;
                }
                else if (maximized > 0)
                {
                    m_window.WindowState = WindowState.Maximized;
                }
                else
                {
                    if (IsResizeable || ignoreWindowResizeMode)
                    {
                        if (width > 0 && height > 0)
                        {
                            m_window.WindowStartupLocation = WindowStartupLocation.Manual;
                            m_window.Left = x;
                            m_window.Top = y;
                            m_window.Width = width;
                            m_window.Height = height;
                        }
                    }
                    else
                    {
                        m_window.WindowStartupLocation = WindowStartupLocation.Manual;
                        m_window.Left = x;
                        m_window.Top = y;
                    }
                }

                if (hidden > 0)
                {
                    m_window.Hide();
                }
            }
        }

        public void Set(XmlDocument xmlDocument = null)
        {
            if (Changed)
            {
                bool minimized = m_window.WindowState == WindowState.Minimized;
                bool maximized = m_window.WindowState == WindowState.Maximized;

                XmlDocument actualXmlDocument = xmlDocument ?? m_settings.GetDocument();

                m_settings.Write(m_sectionName, "X", m_window.Left.ToString(CultureInfo.InvariantCulture), actualXmlDocument);
                m_settings.Write(m_sectionName, "Y", m_window.Top.ToString(CultureInfo.InvariantCulture), actualXmlDocument);
                m_settings.Write(m_sectionName, "Minimized", minimized ? "1" : "0", actualXmlDocument);
                m_settings.Write(m_sectionName, "Maximized", maximized ? "1" : "0", actualXmlDocument);
                m_settings.Write(m_sectionName, "Hidden", !m_window.IsVisible ? "1" : "0", actualXmlDocument);
                m_settings.Write(m_sectionName, "Saved", "1", actualXmlDocument);

                if (IsResizeable && !minimized && !maximized)
                {
                    m_settings.Write(m_sectionName, "Width", m_window.Width.ToString(CultureInfo.InvariantCulture), actualXmlDocument);
                    m_settings.Write(m_sectionName, "Height", m_window.Height.ToString(CultureInfo.InvariantCulture), actualXmlDocument);
                }

                if (xmlDocument == null)
                {
                    m_settings.SaveDocument(actualXmlDocument);
                }
            }
        }

        #region Public Properties

        public bool Changed { private get; set; }

        #endregion

        #region Private Properties

        private bool IsResizeable => m_window.ResizeMode == ResizeMode.CanResizeWithGrip || m_window.ResizeMode == ResizeMode.CanResize;

        #endregion
    }
}