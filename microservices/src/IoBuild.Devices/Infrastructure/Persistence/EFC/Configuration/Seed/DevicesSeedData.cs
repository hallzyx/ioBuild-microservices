using IoBuild.Devices.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Infrastructure.Persistence.EFC.Configuration.Seed;

public static class DevicesSeedData
{
    public static void ApplyDevicesSeedData(this ModelBuilder builder)
    {
        // ==================== SEED DEVICES ====================
        // Devices for Project 1 (Residencial Los Álamos)
        builder.Entity<Device>().HasData(
            new
            {
                Id = 1,
                Name = "Sensor de Temperatura - Torre A",
                Type = "Temperature",
                Location = "Torre A - Piso 5",
                ProjectId = 1,
                Status = "Online",
                MacAddress = "00:11:22:33:44:55"
            },
            new
            {
                Id = 2,
                Name = "Monitor de Humedad - Torre B",
                Type = "Humidity",
                Location = "Torre B - Piso 8",
                ProjectId = 1,
                Status = "Online",
                MacAddress = "00:11:22:33:44:56"
            },
            new
            {
                Id = 3,
                Name = "Medidor de Energía - Áreas Comunes",
                Type = "Energy",
                Location = "Áreas Comunes - Gimnasio",
                ProjectId = 1,
                Status = "Online",
                MacAddress = "00:11:22:33:44:57"
            },
            new
            {
                Id = 9,
                Name = "Medidor de Agua - Torre A",
                Type = "Water",
                Location = "Torre A - Sistema Central",
                ProjectId = 1,
                Status = "Online",
                MacAddress = "00:11:22:33:44:60"
            },
            new
            {
                Id = 10,
                Name = "Control Iluminación - Lobby",
                Type = "Lighting",
                Location = "Lobby Principal",
                ProjectId = 1,
                Status = "Online",
                MacAddress = "00:11:22:33:44:61"
            },
            // Devices for Project 2 (Torres del Pacífico)
            new
            {
                Id = 4,
                Name = "Sensor de Temperatura - Torre 1",
                Type = "Temperature",
                Location = "Torre 1 - Lobby Principal",
                ProjectId = 2,
                Status = "Online",
                MacAddress = "00:11:22:33:44:58"
            },
            new
            {
                Id = 5,
                Name = "Medidor de Agua - Torre 2",
                Type = "Water",
                Location = "Torre 2 - Sistema Central",
                ProjectId = 2,
                Status = "Online",
                MacAddress = "00:11:22:33:44:59"
            },
            new
            {
                Id = 6,
                Name = "Monitor de Energía - Piscina",
                Type = "Energy",
                Location = "Área de Piscina - Terraza",
                ProjectId = 2,
                Status = "Online",
                MacAddress = "00:11:22:33:44:5A"
            },
            new
            {
                Id = 11,
                Name = "Control de Acceso - Entrada Principal",
                Type = "Access Control",
                Location = "Entrada Principal - Torre 1",
                ProjectId = 2,
                Status = "Online",
                MacAddress = "00:11:22:33:44:62"
            },
            new
            {
                Id = 12,
                Name = "Climatización - Áreas Comunes",
                Type = "HVAC",
                Location = "Áreas Comunes",
                ProjectId = 2,
                Status = "Online",
                MacAddress = "00:11:22:33:44:63"
            },
            // Devices for Project 3 (Condominio Las Casuarinas)
            new
            {
                Id = 7,
                Name = "Sensor de Construcción - Área 1",
                Type = "Construction",
                Location = "Zona de Construcción - Sector A",
                ProjectId = 3,
                Status = "Online",
                MacAddress = "00:11:22:33:44:5B"
            },
            new
            {
                Id = 8,
                Name = "Monitor de Seguridad - Perímetro",
                Type = "Security",
                Location = "Perímetro de Obra",
                ProjectId = 3,
                Status = "Online",
                MacAddress = "00:11:22:33:44:5C"
            }
        );

        // ==================== SEED DEVICE LOGS ====================
        // NOTE: DeviceLog.Value is string in the microservice, so numeric values
        // are stored as formatted strings.
        var baseDate = DateTime.UtcNow.AddDays(-30);
        var deviceLogs = new List<object>();

        // Generate daily temperature averages for Device 1 (30 days)
        for (int i = 0; i < 30; i++)
        {
            deviceLogs.Add(new
            {
                Id = 1000 + i,
                DeviceId = 1,
                Timestamp = baseDate.AddDays(i),
                Value = (22.0 + (Math.Sin(i * 0.2) * 2)).ToString("F2"),
                Type = "temperature_daily_avg",
                Metadata = "{}"
            });
        }

        // Generate daily energy totals for Device 3 (30 days)
        for (int i = 0; i < 30; i++)
        {
            deviceLogs.Add(new
            {
                Id = 2000 + i,
                DeviceId = 3,
                Timestamp = baseDate.AddDays(i),
                Value = (800.0 + (Math.Sin(i * 0.3) * 100)).ToString("F2"),
                Type = "energy_daily_total",
                Metadata = "{}"
            });
        }

        // Generate hourly temperature data for Device 4 (7 days x 24 hours = 168 records)
        var recentDate = DateTime.UtcNow.AddDays(-7);
        for (int day = 0; day < 7; day++)
        {
            for (int hour = 0; hour < 24; hour++)
            {
                deviceLogs.Add(new
                {
                    Id = 3000 + (day * 24) + hour,
                    DeviceId = 4,
                    Timestamp = recentDate.AddDays(day).AddHours(hour),
                    Value = (23.0 + (Math.Sin((day * 24 + hour) * 0.1) * 3)).ToString("F2"),
                    Type = "temperature",
                    Metadata = "{}"
                });
            }
        }

        // Generate hourly energy data for Device 6 (7 days x 24 hours = 168 records)
        for (int day = 0; day < 7; day++)
        {
            for (int hour = 0; hour < 24; hour++)
            {
                deviceLogs.Add(new
                {
                    Id = 4000 + (day * 24) + hour,
                    DeviceId = 6,
                    Timestamp = recentDate.AddDays(day).AddHours(hour),
                    Value = (40.0 + (Math.Sin((day * 24 + hour) * 0.15) * 5)).ToString("F2"),
                    Type = "energy",
                    Metadata = "{}"
                });
            }
        }

        // Generate daily water usage data for Device 5 (30 days)
        for (int i = 0; i < 30; i++)
        {
            deviceLogs.Add(new
            {
                Id = 5000 + i,
                DeviceId = 5,
                Timestamp = baseDate.AddDays(i),
                Value = (150.0 + (Math.Sin(i * 0.25) * 20)).ToString("F2"),
                Type = "water_daily_total",
                Metadata = "{}"
            });
        }

        // Generate weekly water usage for Device 9 (7 days)
        for (int i = 0; i < 7; i++)
        {
            deviceLogs.Add(new
            {
                Id = 6000 + i,
                DeviceId = 9,
                Timestamp = recentDate.AddDays(i),
                Value = (120.0 + (Math.Sin(i * 0.4) * 15)).ToString("F2"),
                Type = "water",
                Metadata = "{}"
            });
        }

        builder.Entity<DeviceLog>().HasData(deviceLogs.ToArray());

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
