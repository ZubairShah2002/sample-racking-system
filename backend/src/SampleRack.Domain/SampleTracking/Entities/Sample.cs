using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.Events;
using SampleRack.SharedKernel.Abstractions;
using SampleRack.SharedKernel.Exceptions;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Domain.SampleTracking.Entities;

/// <summary>
/// Carrier for the descriptive fields captured when registering or editing a
/// sample. Grouped to keep factory/update signatures readable; not persisted
/// as an owned type — the fields are flattened onto <see cref="Sample"/>.
/// </summary>
public sealed record SampleProfile(
    string SampleName,
    string? ItemCode,
    string? ProductCategory,
    string? Model,
    string? Size,
    string? Variant,
    string? MaterialType,
    string? SupplierName,
    string? SupplierCode,
    string? PurchaseOrderNumber,
    string? GrnNumber,
    string? SupplierReference,
    DateOnly ReceivedDate,
    TimeOnly? ReceivedTime,
    decimal ReceivedQuantity,
    string? ReceivedBy,
    string? Department,
    string? SampleOwner,
    string? Description,
    string? Purpose,
    string? TestingRequirement,
    string? RequiredTestingType,
    string? StorageRemarks);

/// <summary>
/// A physical sample received by QC and stored in a rack location. Aggregate
/// root for status history, movements, testing records, attachments and the
/// disposal record. Enforces the system's non-negotiable business rules:
/// aging is anchored to the original received date; movement never resets it;
/// testing status and aging are independent; overdue samples are never
/// auto-disposed; disposed samples retain all history.
/// </summary>
public sealed class Sample : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<SampleStatusHistory> _statusHistory = [];
    private readonly List<SampleMovement> _movements = [];
    private readonly List<TestingRecord> _testingRecords = [];
    private readonly List<SampleAttachment> _attachments = [];

    // Identification
    public string SampleCode { get; private set; } = string.Empty;
    public string SampleName { get; private set; } = string.Empty;
    public string? ItemCode { get; private set; }
    public string? ProductCategory { get; private set; }
    public string? Model { get; private set; }
    public string? Size { get; private set; }
    public string? Variant { get; private set; }
    public string? MaterialType { get; private set; }

    // Supplier information
    public string? SupplierName { get; private set; }
    public string? SupplierCode { get; private set; }
    public string? PurchaseOrderNumber { get; private set; }
    public string? GrnNumber { get; private set; }
    public string? SupplierReference { get; private set; }

    // Receiving information
    public DateOnly ReceivedDate { get; private set; }
    public TimeOnly? ReceivedTime { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public string? ReceivedBy { get; private set; }
    public string? Department { get; private set; }
    public string? SampleOwner { get; private set; }

    // Sample information
    public string? Description { get; private set; }
    public string? Purpose { get; private set; }
    public string? TestingRequirement { get; private set; }
    public string? RequiredTestingType { get; private set; }
    public string? StorageRemarks { get; private set; }

    // Storage + status
    public Guid? RackLocationId { get; private set; }
    public string Status { get; private set; } = SampleStatus.Received.ToString();

    // Disposal (one per sample)
    public DisposalRecord? Disposal { get; private set; }

    public IReadOnlyCollection<SampleStatusHistory> StatusHistory => _statusHistory.AsReadOnly();
    public IReadOnlyCollection<SampleMovement> Movements => _movements.AsReadOnly();
    public IReadOnlyCollection<TestingRecord> TestingRecords => _testingRecords.AsReadOnly();
    public IReadOnlyCollection<SampleAttachment> Attachments => _attachments.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    /// <summary>True while the sample physically occupies a rack location.</summary>
    public bool IsStored => RackLocationId.HasValue && Status != SampleStatus.Disposed.ToString();

    public bool IsDisposed => Status == SampleStatus.Disposed.ToString();

    private Sample() { }

    public static Sample Register(
        Guid id,
        TenantId tenantId,
        string sampleCode,
        Guid rackLocationId,
        SampleProfile profile,
        string registeredBy,
        DateTime now)
    {
        if (string.IsNullOrWhiteSpace(sampleCode))
            throw new DomainException("Sample code is required.");
        if (string.IsNullOrWhiteSpace(profile.SampleName))
            throw new DomainException("Sample name is required.");
        if (rackLocationId == Guid.Empty)
            throw new DomainException("A valid rack location is required to register a sample.");
        if (profile.ReceivedQuantity <= 0)
            throw new DomainException("Received quantity must be greater than zero.");

        var sample = new Sample
        {
            Id = id,
            TenantId = tenantId,
            SampleCode = sampleCode.Trim().ToUpperInvariant(),
            RackLocationId = rackLocationId,
            Status = SampleStatus.Received.ToString()
        };

        sample.ApplyProfile(profile);

        sample._statusHistory.Add(SampleStatusHistory.Create(
            Guid.NewGuid(), tenantId, id,
            previousStatus: null,
            newStatus: SampleStatus.Received.ToString(),
            changedBy: registeredBy,
            changedAt: now,
            reason: "Sample registered",
            remarks: null));

        sample.AddDomainEvent(new SampleRegisteredEvent(id, sample.SampleCode, rackLocationId, tenantId.Value));

        return sample;
    }

    public void UpdateDetails(SampleProfile profile)
    {
        if (IsDisposed)
            throw new DomainException("A disposed sample cannot be edited.");
        if (string.IsNullOrWhiteSpace(profile.SampleName))
            throw new DomainException("Sample name is required.");
        if (profile.ReceivedQuantity <= 0)
            throw new DomainException("Received quantity must be greater than zero.");

        ApplyProfile(profile);
    }

    /// <summary>
    /// Changes the sample's operational status, appending to history. Testing and
    /// disposal transitions use their dedicated methods instead of this one.
    /// </summary>
    public void ChangeStatus(SampleStatus newStatus, string changedBy, string? reason, string? remarks, DateTime now)
    {
        if (IsDisposed)
            throw new DomainException("A disposed sample's status cannot be changed.");
        if (newStatus == SampleStatus.Disposed)
            throw new DomainException("Use the disposal workflow to dispose a sample.");

        if (Status == newStatus.ToString())
            return;

        RecordTransition(newStatus, changedBy, reason, remarks, now);
    }

    public TestingRecord RecordTest(
        string testingType,
        string? testingStandard,
        DateTime testingDate,
        string testedBy,
        TestOutcome outcome,
        string? remarks,
        string? defectDescription,
        string? photoEvidenceRef,
        string? reportAttachmentRef,
        DateTime now)
    {
        if (IsDisposed)
            throw new DomainException("A disposed sample cannot be tested.");

        var record = TestingRecord.Create(
            Guid.NewGuid(), TenantId, Id, testingType, testingStandard, testingDate,
            testedBy, outcome, remarks, defectDescription, photoEvidenceRef, reportAttachmentRef, now);

        _testingRecords.Add(record);

        var resultStatus = outcome == TestOutcome.Pass ? SampleStatus.TestedPass : SampleStatus.TestedFail;
        RecordTransition(resultStatus, testedBy, $"Testing recorded: {outcome}", remarks, now);

        return record;
    }

    public SampleMovement MoveTo(
        Guid toLocationId,
        string toLocationCode,
        string? fromLocationCode,
        string movedBy,
        string? reason,
        string? remarks,
        DateTime now)
    {
        if (IsDisposed)
            throw new DomainException("A disposed sample cannot be moved.");
        if (toLocationId == Guid.Empty)
            throw new DomainException("A valid destination location is required.");
        if (RackLocationId == toLocationId)
            throw new DomainException("The sample is already stored in the selected location.");

        var movement = SampleMovement.Create(
            Guid.NewGuid(), TenantId, Id,
            RackLocationId, fromLocationCode,
            toLocationId, toLocationCode,
            movedBy, now, reason, remarks);

        _movements.Add(movement);
        RackLocationId = toLocationId;

        AddDomainEvent(new SampleMovedEvent(Id, SampleCode, fromLocationCode, toLocationCode, TenantId.Value));

        return movement;
    }

    public SampleAttachment AddAttachment(
        string fileName,
        string fileRef,
        string? contentType,
        long? sizeBytes,
        AttachmentCategory category,
        string uploadedBy,
        DateTime now)
    {
        var attachment = SampleAttachment.Create(
            Guid.NewGuid(), TenantId, Id, fileName, fileRef, contentType, sizeBytes, category, uploadedBy, now);
        _attachments.Add(attachment);
        return attachment;
    }

    public void RemoveAttachment(Guid attachmentId)
    {
        var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId)
            ?? throw new NotFoundException("Attachment not found on this sample.");
        _attachments.Remove(attachment);
    }

    public DisposalRecord RequestDisposal(string reason, string requestedBy, string? remarks, DateTime now)
    {
        if (IsDisposed)
            throw new DomainException("The sample has already been disposed.");
        if (Disposal is not null && Disposal.ApprovalStatus == DisposalApprovalStatus.Pending.ToString())
            throw new DomainException("A disposal request is already pending approval for this sample.");

        Disposal = DisposalRecord.CreateRequest(Guid.NewGuid(), TenantId, Id, reason, requestedBy, now, remarks);
        RecordTransition(SampleStatus.ToBeDisposed, requestedBy, "Disposal requested", reason, now);

        return Disposal;
    }

    public void ApproveDisposal(string approvedBy, DateOnly disposalDate, string? remarks, DateTime now)
    {
        if (Disposal is null || Disposal.ApprovalStatus != DisposalApprovalStatus.Pending.ToString())
            throw new DomainException("There is no pending disposal request to approve.");

        Disposal.Approve(approvedBy, disposalDate, now, remarks);

        // Mark disposed and release the rack location. History is retained.
        RecordTransition(SampleStatus.Disposed, approvedBy, "Disposal approved", remarks, now);
        RackLocationId = null;

        AddDomainEvent(new SampleDisposedEvent(Id, SampleCode, TenantId.Value));
    }

    public void RejectDisposal(string approvedBy, string? remarks, DateTime now)
    {
        if (Disposal is null || Disposal.ApprovalStatus != DisposalApprovalStatus.Pending.ToString())
            throw new DomainException("There is no pending disposal request to reject.");

        Disposal.Reject(approvedBy, now, remarks);

        // Rejected disposal: the sample remains stored, placed on hold for review.
        RecordTransition(SampleStatus.OnHold, approvedBy, "Disposal rejected", remarks, now);
    }

    private void RecordTransition(SampleStatus newStatus, string changedBy, string? reason, string? remarks, DateTime now)
    {
        var previous = Status;
        Status = newStatus.ToString();

        _statusHistory.Add(SampleStatusHistory.Create(
            Guid.NewGuid(), TenantId, Id, previous, Status, changedBy, now, reason, remarks));

        AddDomainEvent(new SampleStatusChangedEvent(Id, SampleCode, previous, Status, TenantId.Value));
    }

    private void ApplyProfile(SampleProfile p)
    {
        SampleName = p.SampleName.Trim();
        ItemCode = Normalize(p.ItemCode);
        ProductCategory = p.ProductCategory?.Trim();
        Model = p.Model?.Trim();
        Size = p.Size?.Trim();
        Variant = p.Variant?.Trim();
        MaterialType = p.MaterialType?.Trim();

        SupplierName = p.SupplierName?.Trim();
        SupplierCode = Normalize(p.SupplierCode);
        PurchaseOrderNumber = p.PurchaseOrderNumber?.Trim();
        GrnNumber = p.GrnNumber?.Trim();
        SupplierReference = p.SupplierReference?.Trim();

        ReceivedDate = p.ReceivedDate;
        ReceivedTime = p.ReceivedTime;
        ReceivedQuantity = p.ReceivedQuantity;
        ReceivedBy = p.ReceivedBy?.Trim();
        Department = p.Department?.Trim();
        SampleOwner = p.SampleOwner?.Trim();

        Description = p.Description?.Trim();
        Purpose = p.Purpose?.Trim();
        TestingRequirement = p.TestingRequirement?.Trim();
        RequiredTestingType = p.RequiredTestingType?.Trim();
        StorageRemarks = p.StorageRemarks?.Trim();
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}
