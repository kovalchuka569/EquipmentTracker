using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using Common.Logging;
using Data.Repositories.Consumables.Operations;
using Npgsql;

namespace Core.Services.Consumables.Operations
{
    public class OperationsDataGridService : IOperationsDataGridService
    {
        private readonly IAppLogger<OperationsDataGridService> _logger;
        private readonly IOperationsDataGridRepository _repository;

        public OperationsDataGridService(IAppLogger<OperationsDataGridService> logger, IOperationsDataGridRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }
        
        public async Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName, int materialid)
        {
            _logger.LogInformation("Starting data load from table {Table}", tableName);
            var timer = Stopwatch.StartNew();
            try
            {
                var dataStream = await _repository.LoadDataAsync(tableName, materialid);
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
                _logger.LogError(ex, "Database error in load table {Table} (duration: {Time}ms)",
                    tableName, timer.ElapsedMilliseconds);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load table {Table} (duration: {Time}ms)",
                    tableName, timer.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<int> InsertRecordAsync(string tableName, int materialId, string operationType,
            string dateTime, string quantity, string description, int user)
        {
            _logger.LogInformation("Starting data inserting into material id {materialId}", materialId);
            var timer = Stopwatch.StartNew();
            try
            {
                var newId = await _repository.InsertRecordAsync(tableName, materialId, operationType, dateTime, quantity,
                    description, user);
                _logger.LogInformation("Successfully inserted, new id: {newId} in {Time}ms",
                    newId, timer.ElapsedMilliseconds);
                return newId;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Database error inserting new record, material id: {materialId} (duration: {Time}ms)",
                    materialId, timer.ElapsedMilliseconds);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed inserting new record, material id: {materialId} (duration: {Time}ms)",
                    materialId, timer.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
