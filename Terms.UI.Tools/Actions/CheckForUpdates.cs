using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using Terms.Tools.Settings;
using Terms.Tools.Settings.Interfaces;

namespace Terms.UI.Tools.Actions
{
    public class CheckForUpdates
    {
        #region Private Read-Only Variables

        private readonly string m_updateXmlFilename;
        private readonly int m_waitBeforeCheckingTimeout;

        #endregion

        public CheckForUpdates(string updateXmlFilename, int waitBeforeCheckingTimeout = 1000)
        {
            m_updateXmlFilename = updateXmlFilename;
            m_waitBeforeCheckingTimeout = waitBeforeCheckingTimeout;
        }

        public void Start()
        {
            Thread thread = new Thread(StartChecking);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void StartChecking()
        {
            Thread.Sleep(m_waitBeforeCheckingTimeout);

            if (File.Exists(m_updateXmlFilename))
            {
                File.Delete(m_updateXmlFilename);
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile($"http://www.bunoon.com/updates/{m_updateXmlFilename}", m_updateXmlFilename);
                }

                CheckUpdateFile();
            }
            catch
            {
                BackgroundAction.Run(() => { UpdateNotFound?.Invoke(); });
            }
        }

        private void CheckUpdateFile()
        {
            if (File.Exists(m_updateXmlFilename))
            {
                bool updateFound = true;

                try
                {
                    IXmlSettings updateConfiguration = new XmlSettings(m_updateXmlFilename);

                    Version version = Version.Parse(updateConfiguration.Read("Current", "Version", ""));
                    Version currentVersion = Version.Parse(AssemblyVersion);

                    string released = updateConfiguration.Read("Current", "Released", "");
                    string downloadLink = updateConfiguration.Read("Current", "DownloadLink", "");

                    if (version > currentVersion)
                    {
                        Released = released;
                        NewVersion = version.ToString();
                        DownloadLink = downloadLink;

                        BackgroundAction.Run(() => { UpdateFound?.Invoke(); });
                    }
                    else
                    {
                        updateFound = false;
                    }
                }
                catch
                {
                    updateFound = false;
                }

                if (!updateFound)
                {
                    BackgroundAction.Run(() => { UpdateNotFound?.Invoke(); });
                }
            }
        }

        #region Public Properties

        public Action UpdateFound { set; get; }
        public Action UpdateNotFound { set; get; }
        public string DownloadLink { get; private set; }
        public string Released { get; private set; }
        public string NewVersion { get; private set; }

        #endregion

        #region Private "Assembly Attribute Accessors" Helpers

        private static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        #endregion
    }
}