using System.Collections.ObjectModel;
using System.Dynamic;
using Common.Logging;
using Core.Events.DataGrid;
using Data.Repositories.DataGrid;
using Prism.Events;

namespace Core.Services.DataGrid
{
    public class DataGridService : IDataGridService
    {
        private readonly IAppLogger<DataGridService> _logger;
        private readonly IDataGridRepository _repository;
        private readonly IEventAggregator _eventAggregator;
        
        public DataGridService(IAppLogger<DataGridService> logger, IDataGridRepository repository, IEventAggregator eventAggregator)
        {
            _logger = logger;
            _repository = repository;
            _eventAggregator = eventAggregator;
        }
        public async Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName)
        {
            try
            {
                _logger.LogInformation("Fetching data for table {TableName}", tableName);
                var data = await _repository.GetDataAsync(tableName);
                _logger.LogInformation("Retrieved {Count} records for table {TableName}", data.Count, tableName);
                return data;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch data for table {TableName}", tableName);
                throw;
            }
        }

        public async Task<object> InsertRecordAsync(string tableName, Dictionary<string, object> values)
        {
            try
            {
                _logger.LogInformation("Inserting new record into table {TableName}", tableName);
                var insertedId = await _repository.InsertRecordAsync(tableName, values);
                _logger.LogInformation("Inserted record with ID {Id} into table {TableName}", insertedId, tableName);
                return insertedId;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to insert record into table {TableName}", tableName);
                throw;
            }
        }

        public async Task UpdateRecordAsync(string tableName, object id, Dictionary<string, object> values)
        {
            try
            {
                _logger.LogInformation("Updating record with ID {Id} in table {TableName}", id, tableName);
                await _repository.UpdateRecordAsync(tableName, id, values);
                _logger.LogInformation("Successfully updated record with ID {Id} in table, {TableName}", id, tableName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update record with ID {Id} in table {TableName} in table {Id}", id, tableName);
                throw;
            }
        }

        public async Task DeleteRecordAsync(string tableName, object id)
        {
            try
            {
                _logger.LogInformation("Deleting record with ID {Id} from table {TableName}", id, tableName);
                await _repository.DeleteRecordAsync(tableName, id);
                _logger.LogInformation("Successfully deleted record with ID {Id} in table {TableName}", id, tableName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete record with ID {Id from table {TableName}", id, tableName);
                throw;
            }
        }
        public async Task StartListeningForChangesAsync(CancellationToken cancellationToken, string tableName)
        {
            try
            {
                await foreach (var payload in _repository.StartListeningForChangesAsync(cancellationToken, tableName))
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
    }
}
