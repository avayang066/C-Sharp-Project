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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            Console.WriteLine(
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] .............. Fetching all production logs .............."
            );
            var logs = await _service.GetLogsAsync();
            return Ok(logs);
        }
    }
}
