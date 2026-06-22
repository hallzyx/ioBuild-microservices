namespace IoBuild.Devices.Domain.Model.Aggregates;

/// <summary>
/// Read-model projection of the Projects unit→owner relationship, stored locally in the
/// Devices database so permission checks need NO cross-service HTTP call (ADR-B4, CC-4).
///
/// Populated by <c>UnitOwnerProjectionConsumer</c> whenever a <c>UnitOwnerMatchedEvent</c>
/// is consumed from RabbitMQ.
///
/// Schema: unit_owner_projections (unit_id INT PK, owner_user_id INT, updated_at DATETIME).
/// MySQL-safe: plain PK, NO filtered/partial indexes (ADR-B3 trap avoided).
/// Unique constraint on (unit_id, owner_user_id) enforces idempotency at the DB level.
/// </summary>
public class UnitOwnerProjection
{
    /// <summary>Primary key — the unit's Id from the Projects service.</summary>
    public int UnitId { get; set; }

    /// <summary>The owner user's Id as registered in the IAM service.</summary>
    public int OwnerUserId { get; set; }

    /// <summary>UTC timestamp of the last upsert (set by the consumer on every write).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
