using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.SharedKernel.Abstractions;
using SampleRack.SharedKernel.Exceptions;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Domain.SampleTracking.Entities;

/// <summary>
/// A QC testing record for a sample. Multiple records may exist per sample
/// (retesting); the full history is preserved.
/// </summary>
public sealed class TestingRecord : Entity<Guid>
{
    public Guid SampleId { get; private set; }
    public string TestingType { get; private set; } = string.Empty;
    public string? TestingStandard { get; private set; }
    public DateTime TestingDate { get; private set; }
    public string TestedBy { get; private set; } = string.Empty;
    public string Result { get; private set; } = string.Empty;
    public string? Remarks { get; private set; }
    public string? DefectDescription { get; private set; }
    public string? PhotoEvidenceRef { get; private set; }
    public string? ReportAttachmentRef { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private TestingRecord() { }

    internal static TestingRecord Create(
        Guid id,
        TenantId tenantId,
        Guid sampleId,
        string testingType,
        string? testingStandard,
        DateTime testingDate,
        string testedBy,
        TestOutcome outcome,
        string? remarks,
        string? defectDescription,
        string? photoEvidenceRef,
        string? reportAttachmentRef,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(testingType))
            throw new DomainException("Testing type is required.");

        return new TestingRecord
        {
            Id = id,
            TenantId = tenantId,
            SampleId = sampleId,
            TestingType = testingType.Trim(),
            TestingStandard = testingStandard?.Trim(),
            TestingDate = testingDate,
            TestedBy = string.IsNullOrWhiteSpace(testedBy) ? "system" : testedBy.Trim(),
            Result = outcome.ToString(),
            Remarks = remarks?.Trim(),
            DefectDescription = defectDescription?.Trim(),
            PhotoEvidenceRef = photoEvidenceRef?.Trim(),
            ReportAttachmentRef = reportAttachmentRef?.Trim(),
            CreatedAt = createdAt
        };
    }
}
