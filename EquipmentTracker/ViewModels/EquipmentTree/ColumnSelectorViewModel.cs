using System.Collections.ObjectModel;
using System.ComponentModel;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using System.Windows.Data;
using Common.Logging;
using Core.Events.EquipmentTree;
using Core.Models.Tabs.EquipmentTree;
using Data.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Prism.Events;
using UI.ViewModels.Tabs;
using DbContext = Data.AppDbContext.DbContext;

namespace UI.ViewModels.EquipmentTree;

public class ColumnSelectorViewModel : BindableBase, INavigationAware
{
    private ObservableCollection<Column> _columns;
    private ObservableCollection<Column> _defaultServicesColumns;
    private ObservableCollection<Column> _defaultRepairsColumns;
    private ObservableCollection<object> _selectedColumns;
    
    private readonly DbContext _context;
    private readonly IAppLogger<ColumnSelectorViewModel> _logger;
    
    private Action<bool> _callback;
    
    private IRegionManager _regionManager;
    private IEventAggregator _eventAggregator;
    
    private bool _isInitialized = false;
    private string _tableName;

    public ObservableCollection<Column> Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, value);
    }

    public ObservableCollection<Column> DefaultServicesColumns
    {
        get => _defaultServicesColumns;
        set => SetProperty(ref _defaultServicesColumns, value);
    }
    public ObservableCollection<Column> DefaultRepairsColumns
    {
        get => _defaultRepairsColumns;
        set => SetProperty(ref _defaultRepairsColumns, value);
    }
    public ObservableCollection<object> SelectedColumns
    {
        get => _selectedColumns;
        set => SetProperty(ref _selectedColumns, value);
    }
    
    public DelegateCommand<object> LoadedCommand { get; set; }
    public DelegateCommand CreateTableCommand { get; set; }
    public DelegateCommand CancelCommand { get; set; }
    public DelegateCommand SelectionChangedCommand { get; set; }

    public void OnLoaded(object parameter)
    {
        CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(Columns);
        view.GroupDescriptions.Clear();
        view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
    }

    public ColumnSelectorViewModel(DbContext context, IAppLogger<ColumnSelectorViewModel> logger)
    {
        _context = context;
        _logger = logger;
        
        LoadedCommand = new DelegateCommand<object>(OnLoaded);
        CreateTableCommand = new DelegateCommand(OnCreate);
        
        Columns = new ObservableCollection<Column>();
        SelectedColumns = new ObservableCollection<object>();
        CancelCommand = new DelegateCommand(OnCancel);
        SelectionChangedCommand = new DelegateCommand(OnSelectionChanged);
    }
    
    private async Task<NpgsqlConnection> OpenNewConnectionAsync()
    {
        try
        {
            var connectionString = _context.Database.GetDbConnection().ConnectionString;
                
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
        
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening database connection.");
            throw;
        }
    }


    private void OnSelectionChanged()
    {
        var mandatoryColumns = Columns
            .Where(c => c.IsMandatory || c.ColumnName is "Інвентарний номер" or "Бренд" or "Модель")
            .ToList();
        
        foreach (var mandatoryColumn in mandatoryColumns)
        {
            if (!SelectedColumns.Contains(mandatoryColumn))
            {
                SelectedColumns.Add(mandatoryColumn);
            }
        }
    }

    private async void OnCreate()
    {
        if (!SelectedColumns.Any())
        {
            OnCancel();
            return;
        }
            string tableName = _tableName;
            
            var columns = SelectedColumns.Cast<Column>().Select(c => $"\"{c.ColumnName}\" {c.ColumnType}")
                .ToList();

            string sparePartsTableName = $"{tableName} ЗЧ";
            var sparePartsColumns = new List<string>
            {
                "\"EquipmentId\" INTEGER",
                "\"Назва\" VARCHAR(255)",
                "\"Кількість\" NUMERIC(10, 2)",
                "\"Одиниця\" VARCHAR(255)",
                "\"Серійний номер\" TEXT",
                "\"Примітки\" TEXT",
                $"FOREIGN KEY (\"EquipmentId\") REFERENCES \"UserTables\".\"{tableName}\" (\"id\")"
            };
            var servicesColumns = new List<string>
            {
                "\"Об'єкт\" INTEGER",
                "\"Опис обслуговування\" TEXT",
                "\"Тип обслуговування\" VARCHAR(255)",
                "\"Дата початку\" TIMESTAMP",
                "\"Дата кінця\" TIMESTAMP",
                "\"Витрачено часу\" INTERVAL",
                "\"Статус\" VARCHAR(255)",
                "\"Працівник\" INTEGER",
                $"FOREIGN KEY (\"Об'єкт\") REFERENCES \"UserTables\".\"{tableName}\" (\"id\")"
            };

            var servicesMaterialsColumns = new List<string>
            {
                "\"Робота\" INTEGER",
                "\"Категорія\" VARCHAR(255)",
                "\"Назва\" VARCHAR(255)",
                "\"Одиниця\" VARCHAR(255)", 
                "\"Витрачено\" NUMERIC(10, 2) DEFAULT 0.00",
                $"FOREIGN KEY (\"Робота\") REFERENCES \"UserTables\".\"{tableName} О\" (\"id\")",
            };

            var repairsColumns = new List<string>
            {
                "\"Об'єкт\" INTEGER",
                "\"Опис поломки\" TEXT",
                "\"Дата початку\" TIMESTAMP",
                "\"Дата кінця\" TIMESTAMP",
                "\"Витрачено часу\" INTERVAL",
                "\"Працівник\" INTEGER",
                "\"Статус\" TEXT",
                $"FOREIGN KEY (\"Об'єкт\") REFERENCES \"UserTables\".\"{tableName}\" (\"id\")"
            };

            var repairsMaterialsColumns = new List<string>
            {
                "\"Робота\" INTEGER",
                "\"Категорія\" VARCHAR(255)",
                "\"Назва\" VARCHAR(255)",
                "\"Одиниця\" VARCHAR(255)", 
                "\"Витрачено\" NUMERIC(10, 2) DEFAULT 0.00",
                $"FOREIGN KEY (\"Робота\") REFERENCES \"UserTables\".\"{tableName} Р\" (\"id\")",
            };

            var createTablesQuery =
                $"CREATE EXTENSION IF NOT EXISTS plpgsql; " +
                $"CREATE OR REPLACE FUNCTION \"UserTables\".notify_data_changed() " +
                $"RETURNS TRIGGER AS $$ " +
                $"BEGIN " +
                $"PERFORM pg_notify('data_changed', TG_TABLE_NAME || ':' || TG_OP || ':' || TG_TABLE_SCHEMA); " +
                $"RETURN NULL; " +
                $"END; " +
                $"$$ LANGUAGE plpgsql; " +

                $"CREATE TABLE IF NOT EXISTS \"UserTables\".\"{tableName}\" (Id SERIAL PRIMARY KEY, {string.Join(", ", columns)}, \"IsWriteOff\" BOOLEAN DEFAULT false, \"CopyOfData\" BOOLEAN DEFAULT false); " +

                $"CREATE OR REPLACE TRIGGER \"{tableName}_data_changed_trigger\" " +
                $"AFTER INSERT OR UPDATE OR DELETE ON \"UserTables\".\"{tableName}\" " +
                $"FOR EACH STATEMENT " +
                $"EXECUTE FUNCTION \"UserTables\".notify_data_changed(); " +

                $"CREATE TABLE IF NOT EXISTS \"UserTables\".\"{sparePartsTableName}\" (Id SERIAL PRIMARY KEY, {string.Join(", ", sparePartsColumns)}); " +

                $"CREATE OR REPLACE TRIGGER \"{sparePartsTableName}_data_changed_trigger\" " +
                $"AFTER INSERT OR UPDATE OR DELETE ON \"UserTables\".\"{sparePartsTableName}\" " +
                $"FOR EACH STATEMENT " +
                $"EXECUTE FUNCTION \"UserTables\".notify_data_changed(); " +

                $"CREATE TABLE IF NOT EXISTS \"UserTables\".\"{tableName} О\" (Id SERIAL PRIMARY KEY, {string.Join(", ", servicesColumns)}); " +
                $"CREATE TABLE IF NOT EXISTS \"UserTables\".\"{tableName} ОВМ\" (Id SERIAL PRIMARY KEY, {string.Join(", ", servicesMaterialsColumns)}); " +

                $"CREATE TABLE IF NOT EXISTS \"UserTables\".\"{tableName} Р\" (Id SERIAL PRIMARY KEY, {string.Join(", ", repairsColumns)}); " +
                $"CREATE TABLE IF NOT EXISTS \"UserTables\".\"{tableName} РВМ\" (Id SERIAL PRIMARY KEY, {string.Join(", ", repairsMaterialsColumns)}); ";

            await using var connection = await OpenNewConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            
                try
                {
                    await using var cmd = new NpgsqlCommand(createTablesQuery, connection, transaction);
                    await cmd.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();
                }
                catch (NpgsqlException e)
                {
                    await transaction.RollbackAsync();
                    OnCancel();
                    _logger.LogError(e, "Database error on creating tables");
                    throw;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    OnCancel();
                    _logger.LogError(e, "System error on creating tables");
                    throw;
                }
            
            OnConfirm();
    }


    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
        {
            _regionManager = scopedRegionManager;
        }
        
        if (navigationContext.Parameters["ScopedEventAggregator"] is IEventAggregator scopedEventAgreggator)
        {
            _eventAggregator = scopedEventAgreggator;
        }
        if (navigationContext == null)
        {
            return;
        }

        if (navigationContext.Parameters == null)
        {
            return;
        }

        _tableName = navigationContext.Parameters.GetValue<string>("TableName");
        _callback = navigationContext.Parameters.GetValue<Action<bool>>("Callback");
        
        if (!_isInitialized)
        {
            InitializeColumns();
            _isInitialized = true;
        }
    }
    
    private void InitializeColumns()
    {
        Columns = ColumnDefinitionProvider.GetColumns();
        
        foreach (var column in Columns.Where(c => c.IsMandatory || c.ColumnName is "Інвентарний номер" or "Бренд" or "Модель"))
        {
            SelectedColumns.Add(column);
        }
    }

    private void OnConfirm()
    {
        OnClose();
        _eventAggregator.GetEvent<TableCreatingSuccessfullyEvent>().Publish(true);
    }

    private void OnCancel()
    {
        OnClose();
        _eventAggregator.GetEvent<TableCreatingSuccessfullyEvent>().Publish(false);
    }

    private void OnClose()
    {
        _regionManager.Regions["ColumnSelectorRegion"].RemoveAll();
        SelectedColumns.Clear();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}