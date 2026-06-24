using IoBuild.Devices.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Infrastructure.Persistence.EFC.Configuration.Seed;

public static class DevicesSeedData
{
    public static void ApplyDevicesSeedData(this ModelBuilder builder)
    {
        // ==================== DEMO DEVICES REMOVED ====================
        // WU-3 seed reset: the 12 demo Device rows and all DeviceLog rows previously
        // seeded here have been removed. A fresh stack starts with 0 rows in `devices`.
        // DeviceLog rows FK-cascade from Device, so removing device rows also removes
        // logs on `docker compose down -v`. The RemoveDemoSeed EF migration generates
        // the explicit DeleteData calls that clean Migrate()-path restarts too.

        // ==================== SEED DEVICE TYPE CATALOG ====================
        // Design D4: 5 rows seeded via HasData (migration-tracked, idempotent).
        // AttributesJson uses hardcoded JSON strings (not runtime serialization) so EF
        // detects no pending model changes on each OnModelCreating call.
        // Shape mirrors DeviceCapabilityCatalog.ControllableAttribute (design D2).
        //
        // Note: Spec DT-2 mentions "7 rows" but lists only 5 codes.
        // Design D4 is authoritative: 5 rows. Tests assert 5, not 7.

        // AirConditioner: targetTemperature (number,16-30,C), mode (enum,cooling/heating/fan), power (boolean)
        const string acAttrsJson =
            "[{\"Name\":\"targetTemperature\",\"Type\":\"number\",\"Min\":16.0,\"Max\":30.0,\"Unit\":\"C\",\"EnumMembers\":null}," +
            "{\"Name\":\"mode\",\"Type\":\"enum\",\"Min\":null,\"Max\":null,\"Unit\":null,\"EnumMembers\":[\"cooling\",\"heating\",\"fan\"]}," +
            "{\"Name\":\"power\",\"Type\":\"boolean\",\"Min\":null,\"Max\":null,\"Unit\":null,\"EnumMembers\":null}]";

        // SmartLight: brightness (number,0-100,%), power (boolean)
        const string slAttrsJson =
            "[{\"Name\":\"brightness\",\"Type\":\"number\",\"Min\":0.0,\"Max\":100.0,\"Unit\":\"%\",\"EnumMembers\":null}," +
            "{\"Name\":\"power\",\"Type\":\"boolean\",\"Min\":null,\"Max\":null,\"Unit\":null,\"EnumMembers\":null}]";

        builder.Entity<DeviceType>().HasData(
            new
            {
                Id = 1,
                Code = "SmartMeter",
                DisplayName = "Smart Meter",
                Scope = "floor",
                AttributesJson = "[]"
            },
            new
            {
                Id = 2,
                Code = "WaterSensor",
                DisplayName = "Water Sensor",
                Scope = "floor",
                AttributesJson = "[]"
            },
            new
            {
                Id = 3,
                Code = "SmokeDetector",
                DisplayName = "Smoke Detector",
                Scope = "floor",
                AttributesJson = "[]"
            },
            new
            {
                Id = 4,
                Code = "AirConditioner",
                DisplayName = "Air Conditioner",
                Scope = "unit",
                AttributesJson = acAttrsJson
            },
            new
            {
                Id = 5,
                Code = "SmartLight",
                DisplayName = "Smart Light",
                Scope = "unit",
                AttributesJson = slAttrsJson
            }
        );
    }
}
