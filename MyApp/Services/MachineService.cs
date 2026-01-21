using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Services
{
    public class MachineService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly object _logger; // Placeholder for logger if needed

        public MachineService(ApplicationDbContext dbContext, object logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<Machine>> GetAllMachinesAsync()
        {
            return await _dbContext.Machines.ToListAsync();
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
    }
}
