namespace Core.Interfaces;

public interface IClosableDialog
{
    void SetCloseCommand(DelegateCommand<IDialogResult>? closeCommand);
}