using Terms.UI.Tools.Views;

namespace Terms.Windows.Display;

public partial class Message
{
    public Message(string message)
    {
        InitializeComponent();

        WindowLayout.Setup(this, WindowBorder);

        SetupDisplay(message);
    }

    private void SetupDisplay(string message)
    {
        lblMessage.Text = message;
    }
}