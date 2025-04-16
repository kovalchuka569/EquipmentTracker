using System.Collections.ObjectModel;
using System.Dynamic;
using Common.Logging;
using Data.Repositories.DataGrid;

namespace Core.Services.DataGrid
{
    public class SparePartsService : ISparePartsService
    {
        
        private readonly IAppLogger<SparePartsService> _logger;
        private readonly ISparePartsRepository _repository;

        public SparePartsService(IAppLogger<SparePartsService> logger, ISparePartsRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }
        
        public async Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName, object equipmentId)
        {
            Console.WriteLine("GetDataAsync for spare parts");
            try
            {
                _logger.LogInformation("Fetching data for table {TableName}", tableName);
                var data = await _repository.GetDataAsync(tableName, equipmentId);
                _logger.LogInformation("Retrieved {Count} records for table {TableName}", data.Count, tableName);
                return data;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch data for table {TableName}", tableName);
                throw;
            }
        }
    }
}
