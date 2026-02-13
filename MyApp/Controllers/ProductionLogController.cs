using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ProductionLogController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IProductionLogService _service;

        public ProductionLogController(
            ApplicationDbContext dbContext,
            IProductionLogService service
        )
        {
            _dbContext = dbContext;
            _service = service;
        }

        // 取得生產報告
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            var logs = await _service.GetLogsAsync(page, pageSize);
            return Ok(logs);
        }

        //
        [HttpGet("export")]
        public async Task<IActionResult> ExportAllToExcel()
        {
            var (file, fileName) = await _service.ExportAllLogsToExcelAsync();
            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        // KPI：近一個月總產量
        [HttpGet("kpi-total-output")]
        public async Task<IActionResult> GetKpiTotalOutput()
        {
            var total = await _service.GetTodayTotalOutputAsync();
            return Ok(total);
        }

        // KPI：近一個月平均良率
        [HttpGet("kpi-average-yieldrate")]
        public async Task<IActionResult> GetKpiAverageYieldRate()
        {
            var avg = await _service.GetAverageYieldRateAsync();
            return Ok(avg);
        }
    }
}
