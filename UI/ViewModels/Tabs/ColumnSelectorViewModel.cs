using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Core.Models.Tabs.ProductionEquipmentTree;
using Data.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace UI.ViewModels.Tabs;

public class ColumnSelectorViewModel : BindableBase, INavigationAware
{
    private ObservableCollection<Column> _columns;
    private ObservableCollection<object> _selectedColumns;
    
    private EquipmentTreeViewModel _equipmentTreeViewModel;
    
    private Action<bool> _callback;
    
    private IRegionManager _regionManager;
    private readonly IEventAggregator _eventAggregator;
    
    private bool _isInitialized = false;
    private string _tableName;

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
    public DelegateCommand CancelCommand { get; set; }

    public void OnLoaded(object parameter)
    {
        CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(Columns);
        view.GroupDescriptions.Clear();
        view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
    }

    public ColumnSelectorViewModel(IRegionManager regionManager, EquipmentTreeViewModel equipmentTreeViewModel, IEventAggregator eventAggregator)
    {
        _regionManager = regionManager;
        _equipmentTreeViewModel = equipmentTreeViewModel;
        _eventAggregator = eventAggregator;
        
        LoadedCommand = new DelegateCommand<object>(OnLoaded);
        CreateTableCommand = new DelegateCommand(OnCreate);
        
        Columns = new ObservableCollection<Column>();
        SelectedColumns = new ObservableCollection<object>();
        CancelCommand = new DelegateCommand(OnCancel);
    }

    private void OnCreate()
    {
        if (!SelectedColumns.Any())
        {
            OnCancel();
            return;
        }

        using (var context = new AppDbContext())
        {
            var tableName = _tableName;
            
            var columns = SelectedColumns.Cast<Column>().Select(c => $"\"{c.ColumnName}\" {c.ColumnType}")
                .ToList();
            
            Console.WriteLine(string.Join(", ", columns));
            
            var createTableQuery = $"CREATE TABLE IF NOT EXISTS \"UserTables\".\"{tableName}\" ({string.Join(", ", columns)});";
            context.Database.ExecuteSqlRaw(createTableQuery);
            OnConfirm();
        }
    }


    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.ContainsKey("TableName"))
        {
            string tableName = navigationContext.Parameters.GetValue<string>("TableName");
            _tableName = tableName;
        }
        
        _callback = navigationContext.Parameters.GetValue<Action<bool>>("Callback");
        
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
        
    }

    private void OnConfirm()
    {
        OnClose();
        _callback?.Invoke(true);
    }

    private void OnCancel()
    {
        OnClose();
        _callback?.Invoke(false);
    }

    private void OnClose()
    {
        
        var region = _regionManager.Regions["EquipmentTreeColumnSelectorRegion"];
        if (region != null && region.ActiveViews.Any())
        {
            var view = region.ActiveViews.First();
            region.Remove(view);
        }
        _eventAggregator.GetEvent<ColumnSelectorVisibilityChangedEvent>().Publish(false);
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}