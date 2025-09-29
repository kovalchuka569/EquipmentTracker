namespace Presentation.Contracts;

public interface IDialogHost
{
    bool IsDialogOpen { get; set; }
    
    object? DialogContent { get; set; }
}