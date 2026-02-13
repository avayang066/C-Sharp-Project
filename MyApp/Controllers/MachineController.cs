using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Services;

namespace MyApp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class MachineController : ControllerBase
    {
        private readonly IMachineService _service;

        public MachineController(IMachineService service)
        {
            _service = service;
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

        [HttpPost]
        public async Task<IActionResult> AddMachine([FromBody] MyApp.Models.Machine machine)
        {
            if (machine == null)
                return BadRequest();
            var result = await _service.AddMachineAsync(machine);
            return CreatedAtAction(nameof(GetAllMachines), new { id = result.Id }, result);
        }

        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var success = await _service.ToggleMachineStatusAsync(id);

            if (!success)
            {
                // 找不到機台時回傳 404
                return NotFound(new { message = $"機台 ID {id} 不存在" });
            }

            return Ok(new { message = "狀態已更新" });
        }
    }
}
