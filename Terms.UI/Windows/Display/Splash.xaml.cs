using System;
using System.Threading;
using System.Windows;
using System.Xml;
using Terms.Tools.Settings;
using Terms.Tools.Settings.Interfaces;
using Terms.Tools.Windows;
using Terms.UI.Tools;
using Terms.UI.Tools.Views;

namespace Terms.Windows.Display
{
    public partial class Splash : IDisposable
    {
        private readonly IXmlSettings m_settings;
        private readonly XmlDocument m_xmlDocument;
        private readonly bool m_fadeMainWindowInOutOnStartupShutdown;

        private Timer m_timer;

        public Splash()
        {
            InitializeComponent();

            m_settings = new XmlSettings();
            m_xmlDocument = m_settings.GetDocument();

            WindowLayout.Setup(this);

            Opacity = 0.0;

            int allowMultipleInstancesToBeUsed = Convert.ToInt32(m_settings.Read(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.AllowMultipleInstancesToBeUsed), Settings.MainWindow.AllowMultipleInstancesToBeUsed, m_xmlDocument));
            int fadeMainWindowInOutOnStartupShutdown = Convert.ToInt32(m_settings.Read(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.FadeMainWindowInOutOnStartupShutdown), Settings.MainWindow.FadeMainWindowInOutOnStartupShutdown, m_xmlDocument));

            m_fadeMainWindowInOutOnStartupShutdown = fadeMainWindowInOutOnStartupShutdown > 0;

            if (Processes.IsRunning() && allowMultipleInstancesToBeUsed == 0)
            {
                Application.Current.Shutdown();
            }
            else
            {
                Start();
            }
        }

        public void Dispose()
        {
            m_timer?.Dispose();
        }

        private void Start()
        {
            int showLoadingSplashScreen = Convert.ToInt32(m_settings.Read(Settings.MainWindow.StartUp, nameof(Settings.MainWindow.ShowLoadingSplashScreen), Settings.MainWindow.ShowLoadingSplashScreen));
            if (showLoadingSplashScreen > 0)
            {
                Opacity = 1.0;

                SynchronizationContext synchronizationContext = SynchronizationContext.Current;

                m_timer = new Timer(a =>
                {
                    synchronizationContext.Post(b => ShowMainWindow(), null);

                }, null, TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(-1));
            }
            else
            {
                ShowMainWindow();
            }
        }

        private void ShowMainWindow()
        {
            Main main = new(m_settings, m_xmlDocument, m_fadeMainWindowInOutOnStartupShutdown);
            main.Show();

            Close();
        }
    }
}