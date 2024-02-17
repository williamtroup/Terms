using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Terms.Tools.Settings.Interfaces;
using Terms.Tools.Windows;
using Terms.UI.Tools.Actions;
using Terms.UI.Tools.Views;
using Terms.Windows.Display;

namespace Terms.Windows.Report
{
    public partial class About
    {
        private readonly IXmlSettings m_settings;

        public About(IXmlSettings settings = null, bool viewable = true)
        {
            if (viewable)
            {
                InitializeComponent();

                m_settings = settings;

                WindowLayout.Setup(this, WindowBorder);

                SetupDisplay();
            }
        }

        public static void CheckForUpdates(Window owner)
        {
            About about = new(viewable: false);
            about.StartCheckForUpdates(owner, false);
        }

        private void SetupDisplay()
        {
            lblVersion.Text = $"Version {AssemblyVersion}";
            lblCopyright.Text = AssemblyCopyright;
            txtDescription.Text = AssemblyDescription;
        }

        private void StartCheckForUpdates(Window owner, bool updateLabels = true)
        {
            CheckForUpdates checkForUpdates = new("terms.xml");

            checkForUpdates.UpdateFound = () =>
            {
                if (updateLabels)
                {
                    lblCheckForUpdates.Text = Terms.Resources.UIMessages.CheckForUpdates;
                }

                string message = string.Format(Terms.Resources.UIMessages.NewUpdateAvailable, checkForUpdates.NewVersion, checkForUpdates.Released);

                MessageQuestion messageBox = new(m_settings, message)
                {
                    Topmost = Topmost,
                    Owner = owner
                };

                bool? result = messageBox.ShowDialog();

                if (result != null && result.Value && !string.IsNullOrEmpty(checkForUpdates.DownloadLink))
                {
                    Processes.Start(checkForUpdates.DownloadLink);
                }
            };

            if (updateLabels)
            {
                lblCheckForUpdates.Text = Terms.Resources.UIMessages.CheckingForUpdates;

                checkForUpdates.UpdateNotFound = () =>
                {
                    lblCheckForUpdates.Text = Terms.Resources.UIMessages.NoUpdatesAvailable;
                };
            }

            checkForUpdates.Start();
        }

        private void CheckForUpdatesLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                StartCheckForUpdates(this);
            }
        }

        private void CompanyLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                TextBlock textBlock = (TextBlock) sender;

                Processes.Start(textBlock.Text);

                Close();
            }
        }

        private static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                string description = string.Empty;

                if (attributes.Length != 0)
                {
                    description = ((AssemblyDescriptionAttribute)attributes[0]).Description;
                }

                return description;
            }
        }

        private static string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                string copyright = string.Empty;

                if (attributes.Length != 0)
                {
                    copyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
                }

                return copyright;
            }
        }
    }
}