using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyApp.Data;
using MyApp.Models;

public class ProductionLogGeneratorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ProductionLogGeneratorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private int _executionCount = 0;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                Console.WriteLine(
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] background service running....."
                );

                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // 從資料庫撈出現有機台 Id 清單
                var machineIds = await dbContext.Machines.Select(m => m.Id).ToListAsync();

                if (machineIds == null || machineIds.Count == 0)
                {
                    // 沒有機台，直接跳過這輪
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    continue;
                }

                // 修改為這一行 (傳入 dbContext 和 random)
                var log = await GenerateProductionLogAsync(dbContext, random);

                // 同時，因為現在 log 有可能是 null (當沒有 Active 機台時)，
                // 後面的方法需要加一個 null 檢查，否則會噴 NullReferenceException
                if (log != null)
                {
                    await GenerateAlarmIfNeededAsync(dbContext, log);
                }

                // --- 優化：每執行 10 次才清理一次 ---
                _executionCount++;
                if (_executionCount >= 10)
                {
                    await CleanupOldLogsAsync(dbContext);
                    _executionCount = 0;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task<ProductionLog?> GenerateProductionLogAsync(
        ApplicationDbContext dbContext,
        Random random
    )
    {
        // 1. 直接從資料庫抓取「目前 Active」的機台 Id 清單
        var activeMachineIds = await dbContext
            .Machines.Where(m => m.IsActive)
            .Select(m => m.Id)
            .ToListAsync();

        // 2. 如果目前沒有任何機台是啟用的，就真的不產出資料
        if (!activeMachineIds.Any())
        {
            return null;
        }

        // 3. 從「確定啟用的清單」中隨機挑一個
        int selectedMachineId = activeMachineIds[random.Next(activeMachineIds.Count)];

        // 4. 產生數據邏輯 (保持不變)
        int outputQty = random.Next(10, 101);
        double yieldRate =
            (random.NextDouble() < 0.3)
                ? Math.Round(random.NextDouble() * 0.8, 7)
                : Math.Round(0.8 + random.NextDouble() * 0.2, 7);

        string status = yieldRate > 0.90 ? "Success" : (yieldRate < 0.80 ? "Error" : "Normal");

        var log = new ProductionLog
        {
            MachineId = selectedMachineId,
            Status = status,
            YieldRate = yieldRate,
            OutputQty = outputQty,
            Timestamp = DateTime.Now,
        };

        Console.WriteLine(
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] GENERATE production log for machine {selectedMachineId} SUCCESSFULLY."
        );

        dbContext.ProductionLogs.Add(log);
        await dbContext.SaveChangesAsync();
        return log;
    }

    private async Task GenerateAlarmIfNeededAsync(ApplicationDbContext dbContext, ProductionLog log)
    {
        if (log.YieldRate < 0.80)
        {
            var alarm = new AlarmEvent
            {
                MachineId = log.MachineId,
                ProductionLogId = log.Id,
                AlarmType = "機台異常",
                Message = $"機台 {log.MachineId} 良率過低，良率 = {log.YieldRate * 100:F2}%",
                CreatedAt = DateTime.Now,
            };
            dbContext.AlarmEvents.Add(alarm);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task CleanupOldLogsAsync(ApplicationDbContext dbContext)
    {
        Console.WriteLine($"進入 CleanupOldLogsAsync() 的方法");

        var machineIds = await dbContext.Machines.Select(m => m.Id).ToListAsync();

        foreach (var machineId in machineIds)
        {
            // 1. 找出該機台「由新到舊」排在第 100 筆的那筆資料的 ID
            var lastKeepId = await dbContext
                .ProductionLogs.AsNoTracking()
                .Where(x => x.MachineId == machineId)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .Skip(99) // 跳過前 99 筆
                .FirstOrDefaultAsync();

            // 如果找不到第 100 筆，代表總數小於 100，直接跳過
            if (lastKeepId == 0)
                continue;

            // 2. 執行刪除：只要 ID 比 lastKeepId 小的，就是舊資料
            // 先刪警報
            var oldAlarms = dbContext.AlarmEvents.Where(a =>
                a.MachineId == machineId && a.ProductionLogId < lastKeepId
            );

            // 再刪日誌
            var oldLogs = dbContext.ProductionLogs.Where(x =>
                x.MachineId == machineId && x.Id < lastKeepId
            );

            int count = await oldLogs.CountAsync();
            if (count > 0)
            {
                dbContext.AlarmEvents.RemoveRange(oldAlarms);
                dbContext.ProductionLogs.RemoveRange(oldLogs);
                await dbContext.SaveChangesAsync();

                Console.WriteLine(
                    $"[Cleanup] 機台 {machineId}: 發現第 100 筆 ID 為 {lastKeepId}，已清理 {count} 筆更舊的資料。"
                );
            }
        }
    }
}
