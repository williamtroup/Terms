using System.Windows;
using Terms.Tools.Actions;
using Terms.UI.Tools.Views;

namespace Terms.Windows.Management
{
    public partial class ChangePasswords
    {
        #region Private Read-Only Variables

        private readonly Main m_main;

        #endregion

        public ChangePasswords(Main main)
        {
            InitializeComponent();

            WindowLayout.Setup(this, WindowBorder);

            m_main = main;

            SetupDisplay();
        }

        private void SetupDisplay()
        {
            txtPassword.Focus();

            Password_OnPasswordChanged(null, null);
        }

        #region Private "Change" Events

        private void Button_Change_OnClick(object sender, RoutedEventArgs e)
        {
            string newPassword = Cypher.Encrypt(txtPassword.Password);

            m_main.ChangeAllPasswords(newPassword);

            Close();
        }

        private void Password_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            bChange.IsEnabled = txtPassword.Password.Length > 0;
        }

        #endregion
    }
}