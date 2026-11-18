namespace Toll.Web.UI.Contracts;

public sealed record RegisterVehicleRequest(
    string RegistrationNumber,
    string VehicleType,
    string OwnerName);
