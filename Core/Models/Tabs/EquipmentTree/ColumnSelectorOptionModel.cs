namespace Core.Models.Tabs.EquipmentTree
{
    public class Column : BindableBase
    {
        private string _columnName;
        private string _category;
        private string _columnType;

        public string ColumnName
        {
            get => _columnName;
            set => SetProperty(ref _columnName, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string ColumnType
        {
            get => _columnType;
            set => SetProperty(ref _columnType, value);
        }
    }  
}
