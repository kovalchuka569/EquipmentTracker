namespace Core.Models.DataGrid
{
    public class ColumnMetadata
    {
        // Имя колонки в данных
        public string ColumnName { get; set; }
        
        // Заголовок колонки для отображения
        public string DisplayName { get; set; }
        
        // Тип данных колонки
        public Type DataType { get; set; }
        
        // Строковое представление типа данных для сопоставления
        public string DataTypeString { get; set; }
        
        // Формат отображения (например, для дат, чисел)
        public string Format { get; set; }
        
        // Ширина колонки
        public double Width { get; set; } = double.NaN;
        
        // Порядок отображения
        public int DisplayIndex { get; set; }
        
        // Видимость колонки
        public bool IsVisible { get; set; } = true;
        
        // Разрешено ли редактирование
        public bool AllowEditing { get; set; } = true;
        
        // Разрешена ли сортировка
        public bool AllowSorting { get; set; } = true;
        
        // Конструктор для удобства создания
        public ColumnMetadata(string columnName, Type dataType, string displayName = null)
        {
            ColumnName = columnName;
            DataType = dataType;
            DisplayName = displayName ?? columnName;
            
            // Автоматически определяем строковое представление типа
            if (dataType == typeof(int) || dataType == typeof(long))
                DataTypeString = "integer";
            else if (dataType == typeof(double) || dataType == typeof(decimal))
                DataTypeString = "decimal";
            else if (dataType == typeof(DateTime))
                DataTypeString = "datetime";
            else if (dataType == typeof(bool))
                DataTypeString = "boolean";
            else
                DataTypeString = "string";
        }
    }
}
