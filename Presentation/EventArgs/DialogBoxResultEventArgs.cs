using Presentation.Enums;

namespace Presentation.EventArgs;

public class DialogBoxResultEventArgs(DialogBoxResult dialogResult) : System.EventArgs
{
    public DialogBoxResult DialogResult { get; } = dialogResult;
}