using System.Windows.Controls;
using System.Windows.Documents;

namespace Terms.UI.Tools.Extensions
{
    public static class RichTextBoxExtensions
    {
        public static string Text(this RichTextBox richTextBox)
        {
            return new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
        }
    }
}