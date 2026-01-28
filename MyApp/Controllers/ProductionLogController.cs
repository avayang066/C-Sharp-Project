using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionLogController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ProductionLogService _service;

        public ProductionLogController(ApplicationDbContext dbContext, ProductionLogService service)
        {
            _dbContext = dbContext;
            _service = service;
        }

        // [HttpGet]
        // public async Task<IActionResult> GetAllProductionLogs()
        // {
        //     var logs = await _dbContext.ProductionLogs.ToListAsync();
        //     return Ok(logs);
        // }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            Console.WriteLine("================================== Fetching all production logs...");
            var logs = await _service.GetLogsAsync();
            return Ok(logs);
        }
    }
}
