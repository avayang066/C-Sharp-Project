using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Services
{
    public class FactoryDataService
    {
        private readonly ApplicationDbContext _dbContext;

        public FactoryDataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(
            List<AlarmEvent> alarmEvents,
            List<Machine> machines,
            List<ProductionLog> productionLogs
        )> GetAllFactoryDataAsync()
        {
            var alarmEvents = await _dbContext.AlarmEvents.ToListAsync();
            var machines = await _dbContext.Machines.ToListAsync();
            var productionLogs = await _dbContext.ProductionLogs.ToListAsync();
            return (alarmEvents, machines, productionLogs);
        }
    }
}
