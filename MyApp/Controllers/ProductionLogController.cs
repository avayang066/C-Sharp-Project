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

        // 只保留這一個！
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            var logs = await _service.GetLogsAsync(page, pageSize);
            return Ok(logs);
        }
    }
}
