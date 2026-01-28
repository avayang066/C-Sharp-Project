using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

public class ProductionLogService
{
    private readonly ApplicationDbContext _dbContext;

    public ProductionLogService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProductionLog>> GetLogsAsync()
    {
        return await _dbContext.ProductionLogs.OrderByDescending(x => x.Timestamp).ToListAsync();
    }
}
