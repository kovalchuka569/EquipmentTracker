namespace Core.Events.DataGrid;

public interface IDeletionAgreementResult
{
    bool? Result { get; }
    Action<IDeletionAgreementResult> Callback { get; set; }
}