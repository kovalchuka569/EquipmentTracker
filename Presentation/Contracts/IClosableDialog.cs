using Prism.Commands;
using Prism.Dialogs;

namespace Presentation.Contracts;

public interface IClosableDialog
{
    void SetCloseCommand(DelegateCommand<IDialogResult>? closeCommand);
}