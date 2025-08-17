using Core.Common.Enums;

namespace Core.Models;

public class DialogBoxParameters
{
    public string Title { get; set; } = string.Empty;
    
    public string Message { get; set; } = string.Empty;
    
    public DialogBoxIcon Icon { get; set; } = DialogBoxIcon.None;
    
    public DialogBoxButtons Buttons { get; set; } = DialogBoxButtons.None;

    public string[]? ButtonsText { get; set; } = null;
}