using Microsoft.EntityFrameworkCore;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;

namespace IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;

public class DeviceRepository(DevicesDbContext context) : IDeviceRepository
{
    public async Task AddAsync(Device entity)
    {
        await context.Devices.AddAsync(entity);
    }

    public async Task<Device?> FindByIdAsync(int id)
    {
        return await context.Devices.FindAsync(id);
    }

    public async Task<Device?> FindByMacAddressAsync(string macAddress)
    {
        return await context.Devices.FirstOrDefaultAsync(d => d.MacAddress == macAddress);
    }

    public async Task<IEnumerable<Device>> FindByProjectIdAsync(int projectId)
    {
        return await context.Devices.Where(d => d.ProjectId == projectId).ToListAsync();
    }

    public async Task<IEnumerable<Device>> ListAsync()
    {
        return await context.Devices.ToListAsync();
    }

    public void Remove(Device entity)
    {
        context.Devices.Remove(entity);
    }

    public void Update(Device entity)
    {
        context.Devices.Update(entity);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
