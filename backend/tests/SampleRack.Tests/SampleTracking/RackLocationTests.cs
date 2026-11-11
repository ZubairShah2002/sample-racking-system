using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.Exceptions;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Domain.Tests.SampleTracking;

public class RackLocationTests
{
    private static readonly TenantId Tenant = TenantId.From(Guid.NewGuid());

    [Fact]
    public void Create_normalizes_code_and_computes_sort_order()
    {
        var loc = RackLocation.Create(Guid.NewGuid(), Tenant, "a1-02-03", "a1", 2, 3, "shelf");

        Assert.Equal("A1-02-03", loc.Code);
        Assert.Equal("A1", loc.Zone);
        Assert.Equal(2, loc.Level);
        Assert.Equal(3, loc.Position);
        Assert.Equal(2003, loc.SortOrder);
        Assert.True(loc.IsActive);
    }

    [Theory]
    [InlineData("", "A1", 1, 1)]
    [InlineData("A1-01-01", "", 1, 1)]
    [InlineData("A1-01-01", "A1", 0, 1)]
    [InlineData("A1-01-01", "A1", 1, 0)]
    public void Create_validates_required_fields(string code, string zone, int level, int position)
    {
        Assert.Throws<DomainException>(() =>
            RackLocation.Create(Guid.NewGuid(), Tenant, code, zone, level, position, null));
    }

    [Fact]
    public void Deactivate_and_activate_toggle_state()
    {
        var loc = RackLocation.Create(Guid.NewGuid(), Tenant, "A1-01-01", "A1", 1, 1, null);
        loc.Deactivate();
        Assert.False(loc.IsActive);
        loc.Activate();
        Assert.True(loc.IsActive);
    }

    [Fact]
    public void UpdateDetails_recomputes_sort_order()
    {
        var loc = RackLocation.Create(Guid.NewGuid(), Tenant, "A1-01-01", "A1", 1, 1, null);
        loc.UpdateDetails("A1", 3, 4, "moved");
        Assert.Equal(3, loc.Level);
        Assert.Equal(4, loc.Position);
        Assert.Equal(3004, loc.SortOrder);
    }
}
