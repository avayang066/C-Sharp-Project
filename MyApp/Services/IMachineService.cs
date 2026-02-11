using System.Collections.Generic;
using System.Threading.Tasks;
using MyApp.Models;

namespace MyApp.Services
{
    public interface IMachineService
    {
        Task<List<Machine>> GetAllMachinesAsync();
        Task<List<AlarmEvent>> GetLatestAlarmEventsAsync(int count);
        Task<Machine> AddMachineAsync(Machine machine);
        Task<bool> ToggleMachineStatusAsync(int id);
    }
}
