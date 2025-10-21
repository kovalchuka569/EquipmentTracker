using Presentation.Models;

namespace Presentation.EventArgs;

public class ShowSnackEventArgs
{
    public required Snack Snack { get; set; }
}