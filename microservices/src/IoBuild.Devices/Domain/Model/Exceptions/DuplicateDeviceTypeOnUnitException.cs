namespace IoBuild.Devices.Domain.Model.Exceptions;

/// <summary>
/// Thrown when an owner attempts to add a second device of the same custom type to a unit
/// that already contains a device of that type. Maps to HTTP 409 Conflict at the controller.
/// </summary>
public class DuplicateDeviceTypeOnUnitException(string message) : Exception(message);
