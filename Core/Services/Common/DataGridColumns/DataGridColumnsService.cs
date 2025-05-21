using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Common.Logging;
using Data.Repositories.Common.DataGridColumns;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.Windows.Shared;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Core.Services.Common.DataGridColumns
{
    public class DataGridColumnsService : IDataGridColumnsService
    {
        private IAppLogger<DataGridColumnsService> _logger;
        private DataGridColumnRepository _dataGridColumnRepository;
        private int? _maxLevel;
        private int? _minLevel;
        private int? _currentLevel;

        public DataGridColumnsService(IAppLogger<DataGridColumnsService> logger,
            DataGridColumnRepository dataGridColumnRepository)
        {
            _logger = logger;
            _dataGridColumnRepository = dataGridColumnRepository;
        }

        public void SetBalanceLevels(int? maxLevel, int? minLevel, int? currentLevel)
        {
            _maxLevel = maxLevel;
            _minLevel = minLevel;
            _currentLevel = currentLevel;
        }
        

        public async Task<Dictionary<string, string>> GetColumnTypesAsync(string schema, string tableName)
        {
            _logger.LogInformation("Getting column types for table {TableName}", tableName);
            try {
                var result = await _dataGridColumnRepository.GetColumnTypesAsync(schema, tableName);
                _logger.LogInformation("Retrieved {Count} column types for table {TableName}", 
                    result.Count, tableName);
                return result;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to get column types for table {TableName}", tableName);
                return new Dictionary<string, string>();
            }
        }

        public GridColumn CreateColumnFromDbType(string columnName, string dbType)
        {
            try
            {
                GridColumn column = null;
                
                switch (dbType.ToLower())
                {
                    case "integer":
                    case "int":
                    case "int4":
                    case "int8":
                    case "bigint":
                        column = new GridNumericColumn()
                        {
                            MappingName = columnName,
                            HeaderText = columnName,
                            NumberDecimalDigits = 0,
                            TextAlignment = TextAlignment.Center
                        };
                        break;
                        
                    case "numeric":
                    case "decimal":
                    case "real":
                    case "float4":
                    case "float8":
                    case "double precision":
                        column = new GridNumericColumn
                        {
                            MappingName = columnName,
                            HeaderText = columnName,
                            NumberDecimalDigits = 2,
                            TextAlignment = TextAlignment.Center
                        };
                        break;
                        
                    case "date":
                        column = new GridDateTimeColumn
                        {
                            MappingName = columnName,
                            HeaderText = columnName,
                            Pattern = DateTimePattern.ShortDate,
                            TextAlignment = TextAlignment.Center,
                            MaximumWidth = 120
                        };
                        break;
                        
                    case "timestamp":
                    case "timestamptz":
                        column = new GridDateTimeColumn
                        {
                            MappingName = columnName,
                            HeaderText = columnName,
                            Pattern = DateTimePattern.CustomPattern,
                            TextAlignment = TextAlignment.Center,
                            MaximumWidth = 150
                        };
                        break;
                        
                    case "boolean":
                    case "bool":
                        column = new GridCheckBoxColumn
                        {
                            MappingName = columnName,
                            HeaderText = columnName,
                            TextAlignment = TextAlignment.Center,
                            Width = 80
                        };
                        break;
                        
                    default:
                        column = new GridTextColumn
                        {
                            MappingName = columnName,
                            HeaderText = columnName,
                            TextAlignment = TextAlignment.Center
                        };
                        break;
                }
                
                if (columnName == "id")
                {
                    column.IsHidden = true;
                    column.AllowEditing = false;
                }
                if (columnName == "EquipmentId")
                {   
                    column.IsHidden = true;
                    column.AllowEditing = false;
                }

                if (columnName == "Одиниця")
                {
                    column = new GridComboBoxColumn
                    {
                        MappingName = columnName,
                        HeaderText = columnName,
                        TextAlignment = TextAlignment.Center,
                        ItemsSource = new ObservableCollection<string>
                        {
                            "шт",
                            "кг",
                            "м",
                            "л",
                            "см",
                            "мм",
                            "м²",
                            "м³",
                            "компл",
                            "пара"
                        }
                    };
                }

                if (columnName == "Мінімальний залишок")
                {
                    column.IsHidden = true;
                }
                if (columnName == "Максимальний залишок")
                {
                    column.IsHidden = true;
                }

                if (columnName == "Матеріал")
                {
                    column.IsHidden = true;
                }
                if (columnName == "IsWriteOff")
                {
                    column.IsHidden = true;
                }
                if (columnName == "CopyOfData")
                {
                    column.IsHidden = true;
                }

                return column;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create column {ColumnName} type {DbType}", columnName, dbType);
                return new GridTextColumn { MappingName = columnName, HeaderText = columnName };
            }
        }
        
    }
}
