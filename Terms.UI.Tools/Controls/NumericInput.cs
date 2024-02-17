using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Terms.UI.Tools.Controls.Data;

namespace Terms.UI.Tools.Controls
{
    public static class NumericInput
    {
        private const string ValidInput = "[^0-9]+";

        public static void Create(
            TextBox textBox, 
            int? defaultValueIfBlank = null, 
            int? minimumValueAllowed = null, 
            int? maximumValueAllowed = null, 
            bool validateValuesNow = false)
        {
            bool addEvent = false;

            textBox.PreviewTextInput += TextBox_PreviewTextInput;

            NumericInputData numericInputData = new();

            if (defaultValueIfBlank != null)
            {
                numericInputData.DefaultValueIfBlank = defaultValueIfBlank.Value;

                addEvent = true;
            }

            if (minimumValueAllowed != null)
            {
                numericInputData.MinimumValueAllowed = minimumValueAllowed.Value;

                addEvent = true;
            }

            if (maximumValueAllowed != null)
            {
                numericInputData.MaximumValueAllowed = maximumValueAllowed.Value;

                addEvent = true;
            }

            if (addEvent)
            {
                textBox.Tag = numericInputData;
                textBox.LostFocus += TextBox_LostFocus;

                if (validateValuesNow)
                {
                    TextBox_LostFocus(textBox, null);
                }
            }

            DataObject.AddPastingHandler(textBox, TextBox_OnPaste);
        }

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex.IsMatch(e.Text, ValidInput);
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is NumericInputData numericInputData)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    if (numericInputData.DefaultValueIfBlank != null)
                    {
                        textBox.Text = numericInputData.DefaultValueIfBlank.Value.ToString();
                    }
                }
                else
                {
                    int actualValue = Convert.ToInt32(textBox.Text);

                    if (numericInputData.MinimumValueAllowed != null && actualValue < numericInputData.MinimumValueAllowed.Value)
                    {
                        textBox.Text = numericInputData.MinimumValueAllowed.Value.ToString();
                    }
                    else if (numericInputData.MaximumValueAllowed != null && actualValue > numericInputData.MaximumValueAllowed.Value)
                    {
                        textBox.Text = numericInputData.MaximumValueAllowed.Value.ToString();
                    }
                }
            }
        }

        private static void TextBox_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            string text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;

            if (!string.IsNullOrEmpty(text))
            {
                bool isNotJustNumbers = Regex.IsMatch(text, ValidInput);
                if (isNotJustNumbers)
                {
                    e.CancelCommand();
                }
            }
        }
    }
}