using Microsoft.AspNetCore.Mvc;
using MyApp.Services;
using MyApp.Data;
using System.Threading.Tasks;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachineController : ControllerBase
    {
        private readonly MachineService _service;

        public MachineController(ApplicationDbContext dbContext)
        {
            _service = new MachineService(dbContext, null);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMachines()
        {
            var machines = await _service.GetAllMachinesAsync();
            return Ok(machines);
        }

        [HttpGet("alarms/{count}")]
        public async Task<IActionResult> GetLatestAlarmEvents(int count)
        {
            var alarms = await _service.GetLatestAlarmEventsAsync(count);
            return Ok(alarms);
        }
    }
}