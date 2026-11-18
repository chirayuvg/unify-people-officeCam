using Toll.Web.UI.Contracts;

namespace Toll.Web.UI.Application;

/// <summary>Framework-free validation for the toll vehicle-registration workflow.</summary>
public sealed class VehicleValidationService
{
    public IReadOnlyCollection<string> Validate(RegisterVehicleRequest request)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.RegistrationNumber)) errors.Add("A registration number is required.");
        if (string.IsNullOrWhiteSpace(request.VehicleType)) errors.Add("A vehicle type is required.");
        if (string.IsNullOrWhiteSpace(request.OwnerName)) errors.Add("An owner name is required.");
        return errors;
    }
}
