using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.SharedKernel.Exceptions;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Domain.Tests.SampleTracking;

public class SampleTests
{
    private static readonly TenantId Tenant = TenantId.From(Guid.NewGuid());
    private static readonly DateTime Now = new(2026, 11, 7, 10, 0, 0, DateTimeKind.Utc);

    private static SampleProfile Profile(DateOnly? received = null) => new(
        SampleName: "Bearing housing", ItemCode: "item-001", ProductCategory: "Castings", Model: null,
        Size: null, Variant: null, MaterialType: null, SupplierName: "ABC Supplier", SupplierCode: "abc",
        PurchaseOrderNumber: null, GrnNumber: null, SupplierReference: null,
        ReceivedDate: received ?? new DateOnly(2026, 8, 1), ReceivedTime: null, ReceivedQuantity: 2,
        ReceivedBy: null, Department: null, SampleOwner: null, Description: null, Purpose: null,
        TestingRequirement: null, RequiredTestingType: null, StorageRemarks: null);

    private static Sample Register(Guid locationId) =>
        Sample.Register(Guid.NewGuid(), Tenant, "SMP-2026-0001", locationId, Profile(), "qc@berex", Now);

    [Fact]
    public void Register_sets_received_status_and_initial_history()
    {
        var loc = Guid.NewGuid();
        var sample = Register(loc);

        Assert.Equal("SMP-2026-0001", sample.SampleCode);
        Assert.Equal(SampleStatus.Received.ToString(), sample.Status);
        Assert.Equal(loc, sample.RackLocationId);
        Assert.Equal("ITEM-001", sample.ItemCode); // normalized upper
        Assert.Single(sample.StatusHistory);
        Assert.Null(sample.StatusHistory.First().PreviousStatus);
        Assert.True(sample.IsStored);
    }

    [Fact]
    public void Register_requires_location_and_name()
    {
        Assert.Throws<DomainException>(() =>
            Sample.Register(Guid.NewGuid(), Tenant, "SMP-1", Guid.Empty, Profile(), "u", Now));
    }

    [Fact]
    public void ChangeStatus_appends_history_and_records_previous()
    {
        var sample = Register(Guid.NewGuid());
        sample.ChangeStatus(SampleStatus.PendingTesting, "u", "queued", null, Now);

        Assert.Equal(SampleStatus.PendingTesting.ToString(), sample.Status);
        Assert.Equal(2, sample.StatusHistory.Count);
        var latest = sample.StatusHistory.Last();
        Assert.Equal(SampleStatus.Received.ToString(), latest.PreviousStatus);
        Assert.Equal(SampleStatus.PendingTesting.ToString(), latest.NewStatus);
    }

    [Fact]
    public void RecordTest_fail_sets_tested_fail_and_keeps_testing_record()
    {
        var sample = Register(Guid.NewGuid());
        sample.RecordTest("Dimensional", "ISO-9001", Now, "tester", TestOutcome.Fail, null, "Out of tolerance", null, null, Now);

        Assert.Equal(SampleStatus.TestedFail.ToString(), sample.Status);
        Assert.Single(sample.TestingRecords);
        Assert.Equal("Fail", sample.TestingRecords.First().Result);
    }

    [Fact]
    public void MoveTo_changes_location_but_never_resets_received_date()
    {
        var sample = Register(Guid.NewGuid());
        var originalReceived = sample.ReceivedDate;
        var dest = Guid.NewGuid();

        sample.MoveTo(dest, "A1-03-01", "A1-02-03", "mover", "reorg", null, Now);

        Assert.Equal(dest, sample.RackLocationId);
        Assert.Equal(originalReceived, sample.ReceivedDate);
        Assert.Single(sample.Movements);
        Assert.Equal("A1-03-01", sample.Movements.First().ToLocationCode);
    }

    [Fact]
    public void Disposal_requires_approval_then_releases_location_and_keeps_history()
    {
        var sample = Register(Guid.NewGuid());
        sample.RequestDisposal("End of retention", "requester", null, Now);

        Assert.Equal(SampleStatus.ToBeDisposed.ToString(), sample.Status);
        Assert.NotNull(sample.Disposal);
        Assert.Equal(DisposalApprovalStatus.Pending.ToString(), sample.Disposal!.ApprovalStatus);

        sample.ApproveDisposal("approver", new DateOnly(2026, 11, 7), "ok", Now);

        Assert.Equal(SampleStatus.Disposed.ToString(), sample.Status);
        Assert.Null(sample.RackLocationId); // location released
        Assert.False(sample.IsStored);
        Assert.True(sample.IsDisposed);
        // history retained across the full lifecycle
        Assert.True(sample.StatusHistory.Count >= 3);
    }

    [Fact]
    public void Reject_disposal_puts_sample_on_hold_and_keeps_it_stored()
    {
        var loc = Guid.NewGuid();
        var sample = Register(loc);
        sample.RequestDisposal("maybe", "requester", null, Now);
        sample.RejectDisposal("approver", "keep it", Now);

        Assert.Equal(SampleStatus.OnHold.ToString(), sample.Status);
        Assert.Equal(loc, sample.RackLocationId);
        Assert.Equal(DisposalApprovalStatus.Rejected.ToString(), sample.Disposal!.ApprovalStatus);
    }

    [Fact]
    public void Disposed_sample_cannot_be_moved_or_edited()
    {
        var sample = Register(Guid.NewGuid());
        sample.RequestDisposal("x", "r", null, Now);
        sample.ApproveDisposal("a", new DateOnly(2026, 11, 7), null, Now);

        Assert.Throws<DomainException>(() => sample.MoveTo(Guid.NewGuid(), "A1-01-01", null, "u", null, null, Now));
        Assert.Throws<DomainException>(() => sample.ChangeStatus(SampleStatus.OnHold, "u", null, null, Now));
        Assert.Throws<DomainException>(() => sample.UpdateDetails(Profile()));
    }

    [Fact]
    public void Cannot_request_disposal_twice()
    {
        var sample = Register(Guid.NewGuid());
        sample.RequestDisposal("x", "r", null, Now);
        Assert.Throws<DomainException>(() => sample.RequestDisposal("again", "r", null, Now));
    }

    [Fact]
    public void ChangeStatus_to_disposed_is_rejected_use_workflow()
    {
        var sample = Register(Guid.NewGuid());
        Assert.Throws<DomainException>(() => sample.ChangeStatus(SampleStatus.Disposed, "u", null, null, Now));
    }
}
