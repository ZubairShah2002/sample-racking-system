namespace SampleRack.Application.SampleTracking.DTOs;

/// <summary>Compact sample row for inventory lists, search results and location panels.</summary>
public sealed record SampleSummaryDto(
    Guid Id,
    string SampleCode,
    string SampleName,
    string? ItemCode,
    string? Model,
    string? Size,
    string? Variant,
    string? SupplierName,
    string? SupplierCode,
    Guid? RackLocationId,
    string? RackLocationCode,
    string Status,
    DateOnly ReceivedDate,
    int TotalDaysStored,
    int DaysOverdue,
    string AgingCategory,
    string? LatestTestResult);

public sealed record SampleStatusHistoryDto(
    Guid Id,
    string? PreviousStatus,
    string NewStatus,
    string ChangedBy,
    DateTime ChangedAt,
    string? Reason,
    string? Remarks);

public sealed record SampleMovementDto(
    Guid Id,
    Guid? FromLocationId,
    string? FromLocationCode,
    Guid ToLocationId,
    string ToLocationCode,
    string MovedBy,
    DateTime MovedAt,
    string? Reason,
    string? Remarks);

public sealed record TestingRecordDto(
    Guid Id,
    string TestingType,
    string? TestingStandard,
    DateTime TestingDate,
    string TestedBy,
    string Result,
    string? Remarks,
    string? DefectDescription,
    string? PhotoEvidenceRef,
    string? ReportAttachmentRef,
    DateTime CreatedAt);

public sealed record SampleAttachmentDto(
    Guid Id,
    string FileName,
    string FileRef,
    string? ContentType,
    long? SizeBytes,
    string Category,
    string UploadedBy,
    DateTime UploadedAt);

public sealed record DisposalRecordDto(
    Guid Id,
    string Reason,
    string RequestedBy,
    DateTime RequestedAt,
    string ApprovalStatus,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    DateOnly? DisposalDate,
    string? Remarks);

/// <summary>Full sample record with aging snapshot and all child collections.</summary>
public sealed record SampleDetailDto(
    Guid Id,
    string SampleCode,
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
    string? StorageRemarks,
    Guid? RackLocationId,
    string? RackLocationCode,
    string Status,
    string? LatestTestResult,
    AgingSnapshotDto Aging,
    IReadOnlyList<SampleStatusHistoryDto> StatusHistory,
    IReadOnlyList<SampleMovementDto> Movements,
    IReadOnlyList<TestingRecordDto> TestingRecords,
    IReadOnlyList<SampleAttachmentDto> Attachments,
    DisposalRecordDto? Disposal,
    string CreatedBy,
    DateTime CreatedAt,
    string? ModifiedBy,
    DateTime? ModifiedAt);
