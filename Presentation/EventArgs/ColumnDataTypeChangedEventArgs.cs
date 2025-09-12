using Models.Equipment;

namespace Presentation.EventArgs;

public class ColumnDataTypeChangedEventArgs(ColumnDataType newDataType) : System.EventArgs
{
    public ColumnDataType NewDataType { get; } = newDataType;
}