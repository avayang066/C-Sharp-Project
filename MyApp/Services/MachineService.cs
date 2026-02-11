using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Services
{
    public class MachineService : IMachineService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<MachineService> _logger;

        public MachineService(ApplicationDbContext dbContext, ILogger<MachineService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<Machine>> GetAllMachinesAsync()
        {
            var machines = await _dbContext.Machines.ToListAsync();
            _logger.LogInformation("取得機台數量：{Count}", machines.Count);
            return machines;
        }

        public async Task<List<AlarmEvent>> GetLatestAlarmEventsAsync(int count)
        {
            return await _dbContext
                .AlarmEvents.OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Machine> AddMachineAsync(Machine machine)
        {
            _dbContext.Machines.Add(machine);
            await _dbContext.SaveChangesAsync();
            return machine;
        }

        public async Task<bool> ToggleMachineStatusAsync(int id)
        {
            var machine = await _dbContext.Machines.FindAsync(id);
            if (machine == null)
                return false;

            // 反轉狀態
            machine.IsActive = !machine.IsActive;

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
