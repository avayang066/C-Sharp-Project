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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                Console.WriteLine(
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ===== background service running....."
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

                var log = await GenerateProductionLogAsync(dbContext, machineIds, random);
                await GenerateAlarmIfNeededAsync(dbContext, log);
                await CleanupOldLogsAsync(dbContext, machineIds);
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task<ProductionLog> GenerateProductionLogAsync(
        ApplicationDbContext dbContext,
        List<int> machineIds,
        Random random
    )
    {
        // 隨機挑一個機台 Id
        int selectedMachineId = machineIds[random.Next(machineIds.Count)];

        // 1. 自動產生 OutputQty (例如 10~100 隨機)
        int outputQty = random.Next(10, 101);

        // 2. YieldRate 隨機，但低於0.80機率為30%
        double yieldRate;
        if (random.NextDouble() < 0.3)
        {
            // 0.0~0.8
            yieldRate = Math.Round(random.NextDouble() * 0.8, 7);
        }
        else
        {
            // 0.8~1.0
            yieldRate = Math.Round(0.8 + random.NextDouble() * 0.2, 7);
        }

        // 3. Status依據YieldRate判斷
        string status;
        if (yieldRate > 0.90)
            status = "Success";
        else if (yieldRate < 0.80)
            status = "Error";
        else
            status = "Normal";

        var log = new ProductionLog
        {
            MachineId = selectedMachineId,
            Status = status,
            YieldRate = yieldRate,
            OutputQty = outputQty,
            Timestamp = DateTime.Now,
        };

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

    private async Task CleanupOldLogsAsync(ApplicationDbContext dbContext, List<int> machineIds)
    {
        // 只保留每台機台最新100筆資料
        machineIds = await dbContext.Machines.Select(m => m.Id).ToListAsync();

        foreach (var machineId in machineIds)
        {
            var logIdsToDelete = await dbContext
                .ProductionLogs.Where(x => x.MachineId == machineId)
                .OrderByDescending(x => x.Timestamp)
                .Skip(100)
                .Select(x => x.Id)
                .ToListAsync();

            if (logIdsToDelete.Count > 0)
            {
                // 1. 先刪 AlarmEvent (FK: ProductionLogId)
                var alarmEventsToDelete = dbContext.AlarmEvents.Where(a =>
                    logIdsToDelete.Contains(a.ProductionLogId)
                );
                dbContext.AlarmEvents.RemoveRange(alarmEventsToDelete);

                // 2. 再刪 ProductionLog
                var logsToDelete = dbContext.ProductionLogs.Where(x =>
                    logIdsToDelete.Contains(x.Id)
                );
                dbContext.ProductionLogs.RemoveRange(logsToDelete);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
