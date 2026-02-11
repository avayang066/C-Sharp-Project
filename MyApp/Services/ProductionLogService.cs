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

    // public async Task<List<ProductionLog>> GetLogsAsync()
    // {
    //     return await _dbContext.ProductionLogs.OrderByDescending(x => x.Timestamp).ToListAsync();
    // }

    public async Task<List<ProductionLog>> GetLogsAsync(int page = 1, int pageSize = 20)
    {
        // 預防機制：確保頁碼至少從 1 開始
        if (page <= 0)
            page = 1;

        return await _dbContext
            .ProductionLogs.OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize) // 計算要跳過多少筆
            .Take(pageSize) // 只拿 20 筆
            .ToListAsync();
    }
}
