using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using MyApp.Services;
using Xunit;

public class MachineServiceTests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public MachineServiceTests(Xunit.Abstractions.ITestOutputHelper output)
    {
        _output = output;
    }

    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllMachinesAsync_ReturnsAllMachines()
    {
        // Arrange
        var dbContext = GetDbContext();
        dbContext.Machines.Add(new Machine { Id = 1, MachineName = "TESTA" });
        dbContext.Machines.Add(new Machine { Id = 2, MachineName = "TESTB" });
        await dbContext.SaveChangesAsync();

        var service = new MachineService(dbContext, null);

        // Act
        var result = await service.GetAllMachinesAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetLatestAlarmEventsAsync_ReturnsCorrectCount()
    {
        // Arrange
        var dbContext = GetDbContext();
        dbContext.AlarmEvents.Add(new AlarmEvent { Id = 1, CreatedAt = System.DateTime.Now });
        dbContext.AlarmEvents.Add(
            new AlarmEvent { Id = 2, CreatedAt = System.DateTime.Now.AddMinutes(-1) }
        );
        await dbContext.SaveChangesAsync();

        var service = new MachineService(dbContext, null);

        // Act
        var result = await service.GetLatestAlarmEventsAsync(1);

        _output.WriteLine(
            $"------------------------------------------------------------- 測試取得最新警報 | 警報數量: {result}"
        );

        // Assert
        Assert.Single(result);
    }
}
