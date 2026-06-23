using System.Text.Json;

namespace IoBuild.Devices.Domain.Model.Aggregates;

/// <summary>
/// Owner-scoped custom device type definition.
/// An owner defines a named type with controllable attributes, and can then
/// create devices of this type assigned to their own units.
///
/// Persistence:
///   - Table: owner_custom_device_types
///   - Unique index: (owner_user_id, type_code) — one type-code per owner
///   - AttributesJson: serialized array of OwnerCustomDeviceTypeAttribute
/// </summary>
public class OwnerCustomDeviceType
{
    public int Id { get; private set; }

    /// <summary>Identity of the owner who created this type (string userId from JWT).</summary>
    public string OwnerUserId { get; private set; }

    /// <summary>Short machine-friendly code; unique per owner.</summary>
    public string TypeCode { get; private set; }

    /// <summary>Human-readable display name (1–100 chars).</summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// JSON-serialized array of <see cref="OwnerCustomDeviceTypeAttribute"/>.
    /// Use <see cref="GetAttributes"/> for a deserialized view.
    /// </summary>
    public string AttributesJson { get; private set; }

    // ── EF parameterless constructor (private) ─────────────────────────────
    private OwnerCustomDeviceType()
    {
        OwnerUserId = string.Empty;
        TypeCode = string.Empty;
        DisplayName = string.Empty;
        AttributesJson = string.Empty;
    }

    /// <summary>
    /// Creates a new custom device type with domain-guard validation.
    /// </summary>
    /// <param name="ownerUserId">Owner's JWT user id (non-empty).</param>
    /// <param name="typeCode">Short machine code, unique per owner.</param>
    /// <param name="displayName">Human name (1–100 chars).</param>
    /// <param name="attributes">At least one valid attribute.</param>
    public OwnerCustomDeviceType(
        string ownerUserId,
        string typeCode,
        string displayName,
        IReadOnlyList<OwnerCustomDeviceTypeAttribute> attributes)
    {
        ValidateDisplayName(displayName);
        ValidateAttributes(attributes);

        OwnerUserId = ownerUserId;
        TypeCode = typeCode;
        DisplayName = displayName;
        AttributesJson = JsonSerializer.Serialize(attributes);
    }

    /// <summary>Deserializes the stored JSON back to attribute objects.</summary>
    public IReadOnlyList<OwnerCustomDeviceTypeAttribute> GetAttributes() =>
        JsonSerializer.Deserialize<List<OwnerCustomDeviceTypeAttribute>>(AttributesJson)
        ?? [];

    // ── Domain guards ─────────────────────────────────────────────────────

    private static void ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("displayName must not be empty.", nameof(displayName));

        if (displayName.Length > 100)
            throw new ArgumentException("displayName must not exceed 100 characters.", nameof(displayName));
    }

    private static void ValidateAttributes(IReadOnlyList<OwnerCustomDeviceTypeAttribute> attributes)
    {
        if (attributes is null || attributes.Count == 0)
            throw new ArgumentException("at least one attribute is required.", nameof(attributes));

        foreach (var attr in attributes)
        {
            if (attr.Type == "number" && string.IsNullOrWhiteSpace(attr.Unit))
                throw new ArgumentException(
                    $"Attribute '{attr.Name}': number attributes require a unit.", nameof(attributes));

            if (attr.Type == "enum" && (attr.EnumMembers is null || attr.EnumMembers.Count < 2))
                throw new ArgumentException(
                    $"Attribute '{attr.Name}': enum attributes require at least two enumMembers.", nameof(attributes));
        }
    }
}
