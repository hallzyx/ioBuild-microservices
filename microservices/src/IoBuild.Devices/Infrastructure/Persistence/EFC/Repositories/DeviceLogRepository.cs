using Microsoft.EntityFrameworkCore;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;

namespace IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;

public class DeviceLogRepository(DevicesDbContext context) : IDeviceLogRepository
{
    public async Task AddAsync(DeviceLog entity)
    {
        await context.DeviceLogs.AddAsync(entity);
    }

    public async Task<DeviceLog?> FindByIdAsync(int id)
    {
        return await context.DeviceLogs.FindAsync(id);
    }

    public async Task<IEnumerable<DeviceLog>> FindByDateRangeAsync(DateTime start, DateTime end)
    {
        return await context.DeviceLogs
            .Where(l => l.Timestamp >= start && l.Timestamp <= end)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeviceLog>> FindByDeviceIdAsync(int deviceId)
    {
        return await context.DeviceLogs
            .Where(l => l.DeviceId == deviceId)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeviceLog>> FindByProjectIdAsync(int projectId)
    {
        return await context.DeviceLogs
            .Where(l => context.Devices.Any(d => d.Id == l.DeviceId && d.ProjectId == projectId))
            .ToListAsync();
    }

    public async Task<IEnumerable<DeviceLog>> FindByTypeAsync(string type)
    {
        return await context.DeviceLogs
            .Where(l => l.Type == type)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeviceLog>> ListAsync()
    {
        return await context.DeviceLogs.ToListAsync();
    }

    public void Remove(DeviceLog entity)
    {
        context.DeviceLogs.Remove(entity);
    }

    public void Update(DeviceLog entity)
    {
        context.DeviceLogs.Update(entity);
    }
}
