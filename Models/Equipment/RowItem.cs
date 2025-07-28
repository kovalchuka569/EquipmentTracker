using System.ComponentModel;

public class RowItem : INotifyPropertyChanged
{
    public Guid Id { get; set; }
    private Dictionary<string, object?> _data = new();
    
    public object this[string mappingName]
    {
        get
        {
            if (string.IsNullOrEmpty(mappingName))
                return string.Empty;

            if (_data.TryGetValue(mappingName, out var value))
            {
                return value ?? string.Empty;
            }
            else
            {
                // Возвращаем пустую строку если ключа нет
                return string.Empty;
            }
        }
        set
        {
            if (string.IsNullOrEmpty(mappingName))
                return;

            if (_data.ContainsKey(mappingName))
            {
                if (Equals(_data[mappingName], value))
                    return;
                _data[mappingName] = value;
            }
            else
            {
                _data.Add(mappingName, value);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(mappingName));
        }
    }

    
    public Dictionary<string, object?> Data 
    { 
        get => _data;
        set => _data = value;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
}