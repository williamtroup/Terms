using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Terms.Tools.Extensions;
using Terms.Tools.Settings.Interfaces;
using Terms.UI.Tools;
using Terms.UI.Tools.Actions;
using Terms.UI.Tools.Extensions;
using Terms.UI.Tools.Views;

namespace Terms.Windows.Tools;

public partial class PingConnection
{
    private readonly IXmlSettings m_settings;
    private readonly FilenameDialog m_filenameDialog;
    private readonly string m_address;
    private readonly string m_name;

    private bool m_updateWindowThreadRunning = true;
    private int m_totalTimeBetweenEachPing;
    private int m_totalTimeBetweenEachFailedPing;

    public PingConnection(IXmlSettings settings, FilenameDialog filenameDialog, string address, string name = null)
    {
        InitializeComponent();

        WindowLayout.Setup(this, WindowBorder);

        m_settings = settings;
        m_filenameDialog = filenameDialog;
        m_address = address;
        m_name = name;

        SetupDisplay();
    }

    private void SetupDisplay()
    {
        int totalTimeBetweenEachPing = Convert.ToInt32(m_settings.Read(Settings.PingWindow.PingOptions, nameof(Settings.PingWindow.TotalTimeBetweenEachPing), Settings.PingWindow.TotalTimeBetweenEachPing.ToString()));
        int totalTimeBetweenEachFailedPing = Convert.ToInt32(m_settings.Read(Settings.PingWindow.PingOptions, nameof(Settings.PingWindow.TotalTimeBetweenEachFailedPing), Settings.PingWindow.TotalTimeBetweenEachFailedPing.ToString()));
        int automaticallyScrollToTheBottom = Convert.ToInt32(m_settings.Read(Settings.PingWindow.PingOptions, nameof(Settings.PingWindow.AutomaticallyScrollToTheBottom), Settings.PingWindow.AutomaticallyScrollToTheBottom));

        SetupWindow();

        m_totalTimeBetweenEachPing = totalTimeBetweenEachPing;
        m_totalTimeBetweenEachFailedPing = totalTimeBetweenEachFailedPing;
        chkAutomaticallyScrollToTheBottom.IsChecked = automaticallyScrollToTheBottom > 0;

        StartPinging();
    }

    private void SetupWindow()
    {
        txtPingResults.Document.Blocks.Clear();

        bSave.IsEnabled = false;

        lblPingingAddress.Text = !string.IsNullOrEmpty(m_name)
            ? string.Format(Terms.Resources.UIMessages.PingingNamedAddress, m_name, m_address)
            : string.Format(Terms.Resources.UIMessages.PingingAddress, m_address);
    }

    private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        m_updateWindowThreadRunning = false;

        m_settings.Write(Settings.PingWindow.PingOptions, nameof(Settings.PingWindow.AutomaticallyScrollToTheBottom), chkAutomaticallyScrollToTheBottom.IsReallyChecked().ToNumericString());
    }

    private void StartPinging()
    {
        Thread thread = new(PingingThread);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    private void PingingThread()
    {
        while (m_updateWindowThreadRunning)
        {
            bool success = false;

            using (Ping ping = new())
            {
                PingReply pingReply = ping.ToResult(m_address);

                string result;

                if (pingReply != null && m_updateWindowThreadRunning)
                {
                    if (pingReply.Status != IPStatus.Success)
                    {
                        result = string.Format(Terms.Resources.UIMessages.PingNoSuccess,
                            DateTime.Now,
                            pingReply.Status,
                            pingReply.RoundtripTime,
                            pingReply.Buffer.Length);
                    }
                    else
                    {
                        result = string.Format(Terms.Resources.UIMessages.PingSuccess,
                            DateTime.Now,
                            pingReply.Address,
                            pingReply.Status,
                            pingReply.RoundtripTime,
                            pingReply.Buffer.Length);

                        success = true;
                    }
                }
                else
                {
                    result = string.Format(Terms.Resources.UIMessages.PingAddressUnavailable,
                        DateTime.Now,
                        m_address);
                }

                BackgroundAction.Run(() =>
                {
                    UpdateResults(result, success);
                });
            }

            if (m_updateWindowThreadRunning)
            {
                int timeToWaitForNextPing = success ? m_totalTimeBetweenEachPing : m_totalTimeBetweenEachFailedPing;

                Thread.Sleep(timeToWaitForNextPing);
            }
        }
    }

    private void UpdateResults(string result, bool success)
    {
        result = string.IsNullOrEmpty(txtPingResults.Text()) ? result : $"\r{result}";

        TextRange textRange = new(txtPingResults.Document.ContentEnd, txtPingResults.Document.ContentEnd)
        {
            Text = result
        };

        textRange.ApplyPropertyValue(TextElement.ForegroundProperty, success ? Brushes.Black : Brushes.Gray);
        textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, FontFamily);
        textRange.ApplyPropertyValue(TextElement.FontSizeProperty, FontSize);
        textRange.ApplyPropertyValue(TextElement.FontStyleProperty, success ? FontStyles.Normal : FontStyles.Italic);

        if (chkAutomaticallyScrollToTheBottom.IsReallyChecked())
        {
            txtPingResults.ScrollToEnd();
        }

        bSave.IsEnabled = true;
    }

    private void Button_Stop_OnClick(object sender, RoutedEventArgs e)
    {
        if (m_updateWindowThreadRunning)
        {
            m_updateWindowThreadRunning = false;

            lblPingingAddress.Text = !string.IsNullOrEmpty(m_name)
                ? string.Format(Terms.Resources.UIMessages.PingResultsForNamedAddress, m_name, m_address)
                : string.Format(Terms.Resources.UIMessages.PingResultsForAddress, m_address);

            bStop.Content = Terms.Resources.UIMessages.Restart;
        }
        else
        {
            m_updateWindowThreadRunning = true;

            SetupWindow();
            StartPinging();

            bStop.Content = Terms.Resources.UIMessages.Stop;
        }
    }

    private void Button_SaveAs_OnClick(object sender, RoutedEventArgs e)
    {
        string filename = null;
        bool restart = m_updateWindowThreadRunning;

        m_updateWindowThreadRunning = false;

        if (m_filenameDialog.Save(Terms.Resources.Dialog.TextFileFilter, Terms.Resources.Dialog.SaveAs, ref filename))
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            string[] lines = txtPingResults.Text().Split('\n');

            using (TextWriter textWriter = new StreamWriter(filename))
            {
                foreach (string line in lines)
                {
                    textWriter.WriteLine(line);
                }
            }
        }

        if (restart)
        {
            m_updateWindowThreadRunning = true;

            StartPinging();
        }
    }
}