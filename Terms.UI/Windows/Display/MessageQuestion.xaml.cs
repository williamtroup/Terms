using System.Windows;
using Terms.Tools.Extensions;
using Terms.Tools.Settings.Interfaces;
using Terms.UI.Tools.Extensions;
using Terms.UI.Tools.Models;
using Terms.UI.Tools.Views;

namespace Terms.Windows.Display
{
    public partial class MessageQuestion
    {
        #region Private Read-Only Variables

        private readonly IXmlSettings m_settings;
        private readonly string m_settingSection;
        private readonly string m_settingName;

        #endregion

        public MessageQuestion(IXmlSettings settings, string message, string settingSection = null, string settingName = null)
        {
            InitializeComponent();

            m_settings = settings;
            m_settingSection = settingSection;
            m_settingName = settingName;

            WindowLayout.Setup(this, WindowBorder);

            SetupDisplay(message);
        }

        private void SetupDisplay(string message)
        {
            lblMessage.Text = message;

            if (string.IsNullOrEmpty(m_settingSection) && string.IsNullOrEmpty(m_settingName))
            {
                chkAlwaysShowThisMessage.Visibility = Visibility.Collapsed;
            }
        }

        public MessageQuestionResult Result { get; } = new MessageQuestionResult();

        #region Private "Button" Events

        private void Button_Yes_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Result.Result = true;

            Close();
        }

        private void Button_NO_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Result.Result = false;

            Close();
        }

        #endregion

        #region Private "Window" Events

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!string.IsNullOrEmpty(m_settingSection) && !string.IsNullOrEmpty(m_settingName))
            {
                m_settings.Write(m_settingSection, m_settingName, chkAlwaysShowThisMessage.IsReallyChecked().ToNumericString());
            }
        }

        #endregion

        #region Private "CheckBox" Events

        private void CheckBox_AlwaysShowThisMessage_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Result.SettingResult = chkAlwaysShowThisMessage.IsReallyChecked();
        }

        #endregion
    }
}