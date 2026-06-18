using FluentAssertions;
using IoBuild.Projects.Domain.Model.Aggregates;

namespace IoBuild.Projects.Tests.Domain;

/// <summary>
/// Task 3.1 [RED] — Unit aggregate shape tests (§1.1 / design.md).
/// These tests FAIL until Task 3.2 adds Floor, RoomNumber, OwnerEmail,
/// LinkOwner, AssignOwnerEmail, and ComposeUnitNumber to the Unit class.
/// </summary>
public class UnitAggregateTests
{
    [Fact]
    public void ComposeUnitNumber_ReturnsFloorDashRoom()
    {
        // Scenario: floor 5, room "02" → "5-02"
        var result = Unit.ComposeUnitNumber(5, "02");
        result.Should().Be("5-02");
    }

    [Fact]
    public void NewUnit_StructureCtor_OwnerId_IsNull()
    {
        // Structure-definition ctor: OwnerId is null at creation (REQ-PS-02)
        var unit = new Unit(projectId: 1, floor: 2, roomNumber: "01", ownerEmail: null);
        unit.OwnerId.Should().BeNull("a newly created structural unit has no owner yet");
    }

    [Fact]
    public void LinkOwner_SetsOwnerId()
    {
        var unit = new Unit(projectId: 1, floor: 2, roomNumber: "01", ownerEmail: null);
        unit.LinkOwner(42);
        unit.OwnerId.Should().Be(42);
    }

    [Fact]
    public void AssignOwnerEmail_LowerCasesValue()
    {
        // ADR-D: emails are always stored lower-cased (§3.4)
        var unit = new Unit(projectId: 1, floor: 2, roomNumber: "01", ownerEmail: null);
        unit.AssignOwnerEmail("Alice@Example.COM");
        unit.OwnerEmail.Should().Be("alice@example.com");
    }

    [Fact]
    public void NewUnit_StructureCtor_Floor_And_RoomNumber_AreSet()
    {
        var unit = new Unit(projectId: 3, floor: 4, roomNumber: "05", ownerEmail: "bob@test.com");
        unit.Floor.Should().Be(4);
        unit.RoomNumber.Should().Be("05");
    }

    [Fact]
    public void NewUnit_StructureCtor_OwnerEmail_IsLowerCasedOnAssignment()
    {
        // Structure ctor receiving mixed-case email should lower-case it (ADR-D)
        var unit = new Unit(projectId: 1, floor: 1, roomNumber: "01", ownerEmail: "Carol@Test.COM");
        unit.OwnerEmail.Should().Be("carol@test.com");
    }

    [Fact]
    public void NewUnit_LegacyCtor_Floor_IsZero_RoomNumber_IsUnitNumber()
    {
        // Legacy ctor (CreateUnitCommand path) keeps backward compatibility
        var unit = new Unit(projectId: 1, unitNumber: "A-501", ownerId: 2);
        unit.Floor.Should().Be(0);
        unit.RoomNumber.Should().Be("A-501");
    }
}
