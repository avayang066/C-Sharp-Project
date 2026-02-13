using System;
using System.IO;
using ClosedXML.Excel;
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

    public async Task<List<ProductionLog>> GetLogsAsync(int page = 1, int pageSize = 10)
    {
        // 預防機制：確保頁碼至少從 1 開始
        if (page <= 0)
            page = 1;

        return await _dbContext
            .ProductionLogs.OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize) // 計算要跳過多少筆
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<ProductionLog>> GetAllLogsAsync()
    {
        return await _dbContext.ProductionLogs.OrderByDescending(x => x.Timestamp).ToListAsync();
    }

    public async Task<(byte[] file, string fileName)> ExportAllLogsToExcelAsync()
    {
        var logs = await GetAllLogsAsync();
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("各機台產品報告");

        worksheet.Cell(1, 1).Value = "機台編號";
        worksheet.Cell(1, 2).Value = "產出時間";
        worksheet.Cell(1, 3).Value = "狀態";
        worksheet.Cell(1, 4).Value = "良率";
        worksheet.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var log in logs)
        {
            worksheet.Cell(row, 1).Value = log.MachineId;
            worksheet.Cell(row, 2).Value = log.Timestamp;
            worksheet.Cell(row, 3).Value = log.Status;
            worksheet.Cell(row, 4).Value = log.YieldRate;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"產品良率-{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return (stream.ToArray(), fileName);
    }
}
