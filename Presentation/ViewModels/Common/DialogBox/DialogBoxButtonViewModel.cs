using Common.Enums;
using Prism.Commands;

namespace Presentation.ViewModels.Common.DialogBox;

public class DialogBoxButtonViewModel
{
    public string Text { get; set; } = string.Empty;
    public ButtonStyle Style { get; set; }
    public DelegateCommand Command { get; set; } = null!;
}