namespace Unify.CCC.Domain;

public sealed class CameraConfiguration
{
    public CameraConfiguration(Guid id, Guid tenantId, string name, string connectionAddress, string protocol)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        ConnectionAddress = connectionAddress;
        Protocol = protocol;
    }

    public Guid Id { get; }
    public Guid TenantId { get; }
    public string Name { get; }
    public string ConnectionAddress { get; }
    public string Protocol { get; }
}
