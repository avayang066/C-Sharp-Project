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

        public ProductionLogController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductionLogs()
        {
            var logs = await _dbContext.ProductionLogs.ToListAsync();
            return Ok(logs);
        }
    }
}
