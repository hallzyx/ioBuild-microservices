using System.Text.Json;
using IoBuild.Shared.Domain.Model;

namespace IoBuild.Devices.Domain.Model.Aggregates;

/// <summary>
/// Global device-type catalog entry.
/// Represents a known device type with its controllable attributes, persisted to
/// the <c>device_types</c> table and seeded via HasData in the AddDeviceTypeCatalog migration.
///
/// Design decisions (D1):
///   - <see cref="Code"/> is the natural string key (globally unique, unique index in DB).
///   - <see cref="AttributesJson"/> stores a JSON-serialized array of
///     <see cref="DeviceCapabilityCatalog.ControllableAttribute"/> (shared value object, design D2).
///   - <see cref="Scope"/> narrows applicability: "floor" | "unit" | "both".
///   - Zero attributes are ALLOWED for telemetry-only types (D1 overrides spec DT-1-S7).
///   - Domain guards for non-empty attrs MIRROR OwnerCustomDeviceType except the attrs-count guard.
///
/// Persistence:
///   - Table: device_types
///   - Unique index: Code
///   - AttributesJson: longtext column
///   - Scope: maxlen 20
/// </summary>
public class DeviceType
{
    public int Id { get; private set; }

    /// <summary>Machine-friendly type code; globally unique (e.g. "AirConditioner").</summary>
    public string Code { get; private set; }

    /// <summary>Human-readable label for UI (1–100 chars).</summary>
    public string DisplayName { get; private set; }

    /// <summary>Applicability scope: "floor", "unit", or "both".</summary>
    public string Scope { get; private set; }

    /// <summary>
    /// JSON-serialized array of <see cref="DeviceCapabilityCatalog.ControllableAttribute"/>.
    /// Empty array for telemetry-only types (SmartMeter, WaterSensor, SmokeDetector).
    /// Use <see cref="GetAttributes"/> for a deserialized view.
    /// </summary>
    public string AttributesJson { get; private set; }

    // ── EF parameterless constructor (private) ─────────────────────────────
    private DeviceType()
    {
        Code = string.Empty;
        DisplayName = string.Empty;
        Scope = string.Empty;
        AttributesJson = "[]";
    }

    /// <summary>
    /// Creates a new catalog device type with domain-guard validation.
    /// </summary>
    /// <param name="code">Globally unique type code (non-empty).</param>
    /// <param name="displayName">Human name (1–100 chars).</param>
    /// <param name="scope">Applicability scope: "floor", "unit", or "both".</param>
    /// <param name="attributes">
    /// Controllable attributes. MAY be empty for telemetry-only types.
    /// When non-empty, each attribute is validated per type rules.
    /// </param>
    public DeviceType(
        string code,
        string displayName,
        string scope,
        IReadOnlyList<DeviceCapabilityCatalog.ControllableAttribute> attributes)
    {
        ValidateCode(code);
        ValidateDisplayName(displayName);
        // attributes may be empty (telemetry-only) — no count guard here (design D1)
        if (attributes is not null && attributes.Count > 0)
            ValidateAttributes(attributes);

        Code = code;
        DisplayName = displayName;
        Scope = scope;
        AttributesJson = JsonSerializer.Serialize(attributes ?? Array.Empty<DeviceCapabilityCatalog.ControllableAttribute>());
    }

    /// <summary>Deserializes the stored JSON back to attribute objects.</summary>
    public IReadOnlyList<DeviceCapabilityCatalog.ControllableAttribute> GetAttributes() =>
        JsonSerializer.Deserialize<List<DeviceCapabilityCatalog.ControllableAttribute>>(AttributesJson)
        ?? [];

    // ── Domain guards ─────────────────────────────────────────────────────────

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code must not be null or whitespace.", nameof(code));
    }

    private static void ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("displayName must not be null or whitespace.", nameof(displayName));

        if (displayName.Length > 100)
            throw new ArgumentException("displayName must not exceed 100 characters.", nameof(displayName));
    }

    private static void ValidateAttributes(IReadOnlyList<DeviceCapabilityCatalog.ControllableAttribute> attributes)
    {
        foreach (var attr in attributes)
        {
            if (attr.Type == "number")
            {
                if (string.IsNullOrWhiteSpace(attr.Unit))
                    throw new ArgumentException(
                        $"Attribute '{attr.Name}': number attributes require a non-empty unit.", "attributes");

                if (!attr.Min.HasValue)
                    throw new ArgumentException(
                        $"Attribute '{attr.Name}': number attributes require a min value.", "attributes");

                if (!attr.Max.HasValue)
                    throw new ArgumentException(
                        $"Attribute '{attr.Name}': number attributes require a max value.", "attributes");
            }

            if (attr.Type == "enum" && (attr.EnumMembers is null || attr.EnumMembers.Count < 2))
                throw new ArgumentException(
                    $"Attribute '{attr.Name}': enum attributes require at least two enumMembers.", "attributes");
        }
    }
}
