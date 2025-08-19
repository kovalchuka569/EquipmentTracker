namespace Core.Interfaces;

public interface IDialogHost
{
    bool IsDialogOpen { get; set; }
    
    object? DialogContent { get; set; }
}