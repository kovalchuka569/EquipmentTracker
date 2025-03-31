using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Core.Models.Tabs.ProductionEquipmentTree;
using Data.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace UI.ViewModels.Tabs;

public class ColumnSelectorViewModel : BindableBase
{
    private ObservableCollection<Column> _columns;
    private ObservableCollection<object> _selectedColumns;

    public ObservableCollection<Column> Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, value);
    }

    public ObservableCollection<object> SelectedColumns
    {
        get => _selectedColumns;
        set => SetProperty(ref _selectedColumns, value);
    }
    
    public DelegateCommand<object> LoadedCommand { get; set; }
    public DelegateCommand CreateTableCommand { get; set; }

    public void OnLoaded(object parameter)
    {
        CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(Columns);
        
        view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
    }

    public ColumnSelectorViewModel()
    {
        Columns = new ObservableCollection<Column>();
        SelectedColumns = new ObservableCollection<object>();
        
        Columns.Add(new Column{ColumnName = "Інвентарний номер", ColumnType = "TEXT", Category = "Основні характеристики"});
        Columns.Add(new Column{ColumnName = "Бренд", ColumnType = "VARCHAR(255)", Category = "Основні характеристики"});
        Columns.Add(new Column{ColumnName = "Модель", ColumnType = "VARCHAR(255)", Category = "Основні характеристики"});
        Columns.Add(new Column{ColumnName = "Серійний номер", ColumnType = "TEXT", Category = "Основні характеристики"});
        Columns.Add(new Column{ColumnName = "Клас", ColumnType = "VARCHAR(255)", Category = "Основні характеристики"});
        Columns.Add(new Column{ColumnName = "Рік", ColumnType = "INTEGER", Category = "Основні характеристики"});
        
        Columns.Add(new Column{ColumnName = "Розмір (см)", ColumnType = "DECIMAL(10,2)", Category = "Фізичні характеристики"});
        Columns.Add(new Column{ColumnName = "Вага (кг)", ColumnType = "DECIMAL(10,2)", Category = "Фізичні характеристики"});
        
        Columns.Add(new Column{ColumnName = "Поверх", ColumnType = "VARCHAR(255)", Category = "Локація"});
        Columns.Add(new Column{ColumnName = "Відділ", ColumnType = "VARCHAR(255)", Category = "Локація"});
        Columns.Add(new Column{ColumnName = "Кімната", ColumnType = "VARCHAR(255)", Category = "Локація"});
        
        Columns.Add(new Column{ColumnName = "Споживання (кв/год)", ColumnType = "DECIMAL(10,2)", Category = "Технічні характеристики"});
        Columns.Add(new Column{ColumnName = "Напруга (В)", ColumnType = "DECIMAL(10,2)", Category = "Технічні характеристики"});
        Columns.Add(new Column{ColumnName = "Вода (л/год)", ColumnType = "DECIMAL(10,2)", Category = "Технічні характеристики"});
        Columns.Add(new Column{ColumnName = "Повітря (л/год)", ColumnType = "DECIMAL(10,2)", Category = "Технічні характеристики"});
        
        Columns.Add(new Column{ColumnName = "Балансова вартість", ColumnType = "DECIMAL(10,2)", Category = "Фінансові характеристики"});
        
        Columns.Add(new Column{ColumnName = "Нотатки", ColumnType = "TEXT", Category = "Інше"});
        Columns.Add(new Column{ColumnName = "Відповідальний", ColumnType = "TEXT", Category = "Інше"});
        

        
        LoadedCommand = new DelegateCommand<object>(OnLoaded);
        CreateTableCommand = new DelegateCommand(OnCreate);
    }

    private void OnCreate()
    {
        if (!SelectedColumns.Any()) return;

        using (var context = new AppDbContext())
        {
            var tableName = "TestTable";
            
            var columns = SelectedColumns.Cast<Column>().Select(c => $"\"{c.ColumnName}\" {c.ColumnType}")
                .ToList();
            
            var createTableQuery = $"CREATE TABLE IF NOT EXISTS \"UserTables\".\"{tableName}\" ({string.Join(", ", columns)});";
            context.Database.ExecuteSqlRaw(createTableQuery);
            Console.WriteLine($"Created table: {tableName}");
        }
    }
   
}