using Common.Enums;

namespace Presentation.Models;

public class DialogBoxButtonInfo
{
    public string Text { get; set; } = string.Empty;
    public ButtonStyle Style { get; set; } = ButtonStyle.Default;
}