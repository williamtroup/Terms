using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Terms.UI.Tools.Controls
{
    public class TextFieldInput(TextBox textBox)
    {
        #region Private Read-Only Variables

        private readonly TextBox m_textBox = textBox;

        #endregion

        #region Private Variables

        private Timer m_textChangedTimer;

        #endregion

        public void Focus()
        {
            m_textBox.Focus();
        }

        #region Property Text

        public string Text => IsPlaceHolderSet ? "" : m_textBox.Text;

        #endregion

        #region Property IsFocused

        public bool IsFocused => m_textBox.IsFocused;

        #endregion

        #region Property TextChanged

        private Action m_textChanged;

        public Action TextChanged
        {
            private get => m_textChanged;
            set
            {
                m_textChanged = value;

                m_textBox.TextChanged += TextBox_TextChanged;
            }
        }

        #endregion

        #region Property PlaceHolder

        private string m_placeHolder;

        public string PlaceHolder
        {
            private get => m_placeHolder;
            set
            {
                m_placeHolder = value;

                m_textBox.GotFocus += TextBox_GotFocus;
                m_textBox.LostFocus += TextBox_LostFocus;

                if (!m_textBox.IsFocused)
                {
                    TextBox_LostFocus(null, null);
                }
            }
        }

        #endregion

        #region Property PlaceHolderTimeout

        public int PlaceHolderTimeout { private get; set; } = 250;

        #endregion

        #region Property IsPlaceHolderSet

        private bool IsPlaceHolderSet => string.Equals(m_textBox.Text, PlaceHolder, StringComparison.CurrentCultureIgnoreCase);

        #endregion

        #region Private "Event" Helpers

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (m_textBox.Text == PlaceHolder)
            {
                m_textBox.Text = string.Empty;
                m_textBox.Foreground = Brushes.Black;
                m_textBox.FontStyle = FontStyles.Normal;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(m_textBox.Text) || IsPlaceHolderSet)
            {
                m_textBox.Text = PlaceHolder;
                m_textBox.Foreground = Brushes.Gray;
                m_textBox.FontStyle = FontStyles.Italic;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                m_textChangedTimer?.Dispose();

                SynchronizationContext synchronizationContext = SynchronizationContext.Current;

                m_textChangedTimer = new Timer(a =>
                {
                    synchronizationContext.Post(b => { TextChanged(); }, null);

                }, null, TimeSpan.FromMilliseconds(PlaceHolderTimeout), TimeSpan.FromMilliseconds(-1));
            }
        }

        #endregion
    }
}