using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using Common.Logging;
using Core.Events.DataGrid;
using Data.AppDbContext;
using Data.Repositories.Consumables;
using Npgsql;

namespace Core.Services.Consumables
{
    public class ConsumablesDataGridService : IConsumablesDataGridService
    {
        private IAppLogger<ConsumablesDataGridService> _logger;
        private IConsumablesDataGridRepository _repository;
        private IEventAggregator _eventAggregator;

        public ConsumablesDataGridService(IAppLogger<ConsumablesDataGridService> logger, IConsumablesDataGridRepository repository, IEventAggregator eventAggregator)
        {
            _logger = logger;
            _repository = repository;
            _eventAggregator = eventAggregator;
        }
        
        public async Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName)
        {
            _logger.LogInformation("Starting data load from table {Table}", tableName);
            var timer = Stopwatch.StartNew();
            try
            {
                var dataStream = await _repository.GetDataAsync(tableName);
                var result = new ObservableCollection<ExpandoObject>();
                
                    await foreach (var item in dataStream)
                    {
                        result.Add(item);
                    }
                    
                    _logger.LogInformation("Successfully loaded {Count} rows in {Time}ms", 
                        result.Count, timer.ElapsedMilliseconds);
                    
                    return result;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Failed to load table {Table} (duration: {Time}ms)", 
                    tableName, timer.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task StartListeningForChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var payload in _repository.StartListeningForChangesAsync(cancellationToken))
                {
                    _eventAggregator.GetEvent<DataChangedEvent>().Publish(payload);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StartListeningForChangesAsync");
                throw;
            }
        }

        public async Task<(int? min, int? max)> GetDataMinMaxAsync(string tableName, int recordId)
        {
            try
            {
               return await _repository.GetDataMinMaxAsync(tableName, recordId);
            }
            catch (NpgsqlException e)
            {
                _logger.LogError(e, "Database error in getting Min/Max level data");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System error in getting Min/Max level data");
                throw;
            }
        }

        public async Task UpdateMinLevelAsync(string tableName, int recordId, int? min)
        {
            try
            {
                await _repository.UpdateMinLevelAsync(tableName, recordId, min);
            }
            catch (NpgsqlException e)
            {
                _logger.LogError(e, "Database error in updating Min level data");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System error in updating Min level data");
                throw;
            }
        }
        public async Task UpdateMaxLevelAsync(string tableName, int recordId, int? max)
        {
            try
            {
                await _repository.UpdateMaxLevelAsync(tableName, recordId, max);
            }
            catch (NpgsqlException e)
            {
                _logger.LogError(e, "Database error in updating Max level data");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System error in updating Max level data");
                throw;
            }
        }

        public async Task<int> UpdateQuantityAsync(string tableName, int materialId, string quantity, string operation)
        {
            try
            {
                if (operation == "Списання")
                {
                   var newQuantity = await _repository.DecreaseQuantityAsync(tableName, materialId, quantity);
                   return newQuantity;
                }
                else if (operation == "Прихід")
                {
                    var newQuantity = await _repository.IncreaseQuantityAsync(tableName, materialId, quantity);
                    return newQuantity;
                }

                return 0;
            }
            catch (NpgsqlException e)
            {
                _logger.LogError(e, "Database error in updating quantity data");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System error in updating quantity data");
                throw;
            }
        }
    }
}
