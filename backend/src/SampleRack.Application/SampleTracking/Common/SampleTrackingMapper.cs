using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.ValueObjects;

namespace SampleRack.Application.SampleTracking.Common;

/// <summary>
/// Maps sample tracking aggregates to DTOs, computing aging on the fly so every
/// surface reports identical, always-current figures.
/// </summary>
public static class SampleTrackingMapper
{
    public static AgingSnapshotDto ToAgingDto(DateOnly receivedDate, DateOnly today, AgingThresholds thresholds)
    {
        var snap = AgingCalculator.Calculate(receivedDate, today, thresholds);
        return new AgingSnapshotDto(
            snap.ReceivedDate,
            snap.CurrentDate,
            snap.TotalDaysStored,
            snap.StorageAgeMonths,
            snap.DaysRemainingBeforeOverdue,
            snap.DaysOverdue,
            snap.Category.ToString());
    }

    public static SampleSummaryDto ToSummaryDto(Sample s, DateOnly today, AgingThresholds thresholds, string? locationCode)
    {
        var snap = AgingCalculator.Calculate(s.ReceivedDate, today, thresholds);
        return new SampleSummaryDto(
            s.Id,
            s.SampleCode,
            s.SampleName,
            s.ItemCode,
            s.Model,
            s.Size,
            s.Variant,
            s.SupplierName,
            s.SupplierCode,
            s.RackLocationId,
            locationCode,
            s.Status,
            s.ReceivedDate,
            snap.TotalDaysStored,
            snap.DaysOverdue,
            snap.Category.ToString(),
            DeriveLatestTestResult(s));
    }

    public static SampleDetailDto ToDetailDto(Sample s, DateOnly today, AgingThresholds thresholds, string? locationCode)
    {
        var latestTest = s.TestingRecords
            .OrderByDescending(t => t.TestingDate)
            .ThenByDescending(t => t.CreatedAt)
            .FirstOrDefault()?.Result ?? DeriveLatestTestResult(s);

        return new SampleDetailDto(
            s.Id,
            s.SampleCode,
            s.SampleName,
            s.ItemCode,
            s.ProductCategory,
            s.Model,
            s.Size,
            s.Variant,
            s.MaterialType,
            s.SupplierName,
            s.SupplierCode,
            s.PurchaseOrderNumber,
            s.GrnNumber,
            s.SupplierReference,
            s.ReceivedDate,
            s.ReceivedTime,
            s.ReceivedQuantity,
            s.ReceivedBy,
            s.Department,
            s.SampleOwner,
            s.Description,
            s.Purpose,
            s.TestingRequirement,
            s.RequiredTestingType,
            s.StorageRemarks,
            s.RackLocationId,
            locationCode,
            s.Status,
            latestTest,
            ToAgingDto(s.ReceivedDate, today, thresholds),
            s.StatusHistory
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new SampleStatusHistoryDto(
                    h.Id, h.PreviousStatus, h.NewStatus, h.ChangedBy, h.ChangedAt, h.Reason, h.Remarks))
                .ToList(),
            s.Movements
                .OrderByDescending(m => m.MovedAt)
                .Select(m => new SampleMovementDto(
                    m.Id, m.FromLocationId, m.FromLocationCode, m.ToLocationId, m.ToLocationCode,
                    m.MovedBy, m.MovedAt, m.Reason, m.Remarks))
                .ToList(),
            s.TestingRecords
                .OrderByDescending(t => t.TestingDate)
                .Select(t => new TestingRecordDto(
                    t.Id, t.TestingType, t.TestingStandard, t.TestingDate, t.TestedBy, t.Result,
                    t.Remarks, t.DefectDescription, t.PhotoEvidenceRef, t.ReportAttachmentRef, t.CreatedAt))
                .ToList(),
            s.Attachments
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new SampleAttachmentDto(
                    a.Id, a.FileName, a.FileRef, a.ContentType, a.SizeBytes, a.Category, a.UploadedBy, a.UploadedAt))
                .ToList(),
            s.Disposal is null
                ? null
                : new DisposalRecordDto(
                    s.Disposal.Id, s.Disposal.Reason, s.Disposal.RequestedBy, s.Disposal.RequestedAt,
                    s.Disposal.ApprovalStatus, s.Disposal.ApprovedBy, s.Disposal.ApprovedAt,
                    s.Disposal.DisposalDate, s.Disposal.Remarks),
            s.CreatedBy,
            s.CreatedAt,
            s.ModifiedBy,
            s.ModifiedAt);
    }

    /// <summary>
    /// Best-effort latest test result for summary rows that do not eager-load
    /// testing records: reflects the current tested status when present.
    /// </summary>
    private static string? DeriveLatestTestResult(Sample s)
        => s.Status switch
        {
            var x when x == SampleStatus.TestedPass.ToString() => TestOutcome.Pass.ToString(),
            var x when x == SampleStatus.TestedFail.ToString() => TestOutcome.Fail.ToString(),
            _ => null
        };
}
