using System.Collections.Generic;
using System.Threading.Tasks;
using MyApp.Models;
using MyApp.Models.Dto;

namespace MyApp.Services
{
    public interface IProductionLogService
    {
        Task<List<ProductionLogSimpleDto>> GetLogsAsync(int page = 1, int pageSize = 10);
        Task<(byte[] file, string fileName)> ExportAllLogsToExcelAsync();
        Task<int> GetTodayTotalOutputAsync();
        Task<double> GetAverageYieldRateAsync();
        Task<List<ProductionLog>> GetAllLogsAsync();
    }
}
