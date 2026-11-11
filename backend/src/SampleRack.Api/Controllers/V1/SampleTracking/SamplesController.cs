using Asp.Versioning;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Commands.AddSampleAttachment;
using SampleRack.Application.SampleTracking.Commands.ApproveDisposal;
using SampleRack.Application.SampleTracking.Commands.ChangeSampleStatus;
using SampleRack.Application.SampleTracking.Commands.MoveSample;
using SampleRack.Application.SampleTracking.Commands.RecordTest;
using SampleRack.Application.SampleTracking.Commands.RegisterSample;
using SampleRack.Application.SampleTracking.Commands.RejectDisposal;
using SampleRack.Application.SampleTracking.Commands.RemoveSampleAttachment;
using SampleRack.Application.SampleTracking.Commands.RequestDisposal;
using SampleRack.Application.SampleTracking.Commands.UpdateSample;
using SampleRack.Application.SampleTracking.Queries.GetSampleByCode;
using SampleRack.Application.SampleTracking.Queries.GetSampleDetail;
using SampleRack.Application.SampleTracking.Queries.ListSamples;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers.V1.SampleTracking;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/samples")]
[Authorize]
public sealed class SamplesController : SampleTrackingControllerBase
{
    private const string AttachmentBucket = "sample-attachments";
    private readonly IFileStorageService _fileStorage;

    public SamplesController(ISender sender, IFileStorageService fileStorage) : base(sender)
    {
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? agingCategory,
        [FromQuery] string? supplierName,
        [FromQuery] string? productCategory,
        [FromQuery] string? department,
        [FromQuery] string? locationCode,
        [FromQuery] DateOnly? receivedFrom,
        [FromQuery] DateOnly? receivedTo,
        [FromQuery] bool includeDisposed = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Respond(await Sender.Send(new ListSamplesQuery(
            search, status, agingCategory, supplierName, productCategory, department, locationCode,
            receivedFrom, receivedTo, includeDisposed, sortBy, sortDir, page, pageSize), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new GetSampleDetailQuery(id), cancellationToken));

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new GetSampleByCodeQuery(code), cancellationToken));

    [HttpPost]
    [Authorize(Roles = SampleTrackingRoles.QcUser)]
    public async Task<IActionResult> Register([FromBody] RegisterSampleRequest r, CancellationToken cancellationToken)
        => RespondCreated(await Sender.Send(new RegisterSampleCommand(
            r.RackLocationId, r.SampleName, r.ItemCode, r.ProductCategory, r.Model, r.Size, r.Variant, r.MaterialType,
            r.SupplierName, r.SupplierCode, r.PurchaseOrderNumber, r.GrnNumber, r.SupplierReference,
            r.ReceivedDate, r.ReceivedTime, r.ReceivedQuantity, r.ReceivedBy, r.Department, r.SampleOwner,
            r.Description, r.Purpose, r.TestingRequirement, r.RequiredTestingType, r.StorageRemarks), cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SampleTrackingRoles.QcUser)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSampleRequest r, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new UpdateSampleCommand(
            id, r.SampleName, r.ItemCode, r.ProductCategory, r.Model, r.Size, r.Variant, r.MaterialType,
            r.SupplierName, r.SupplierCode, r.PurchaseOrderNumber, r.GrnNumber, r.SupplierReference,
            r.ReceivedDate, r.ReceivedTime, r.ReceivedQuantity, r.ReceivedBy, r.Department, r.SampleOwner,
            r.Description, r.Purpose, r.TestingRequirement, r.RequiredTestingType, r.StorageRemarks), cancellationToken));

    [HttpPost("{id:guid}/status")]
    [Authorize(Roles = SampleTrackingRoles.QcUser)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest r, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new ChangeSampleStatusCommand(id, r.Status, r.Reason, r.Remarks), cancellationToken));

    [HttpPost("{id:guid}/move")]
    [Authorize(Roles = SampleTrackingRoles.QcUser)]
    public async Task<IActionResult> Move(Guid id, [FromBody] MoveSampleRequest r, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new MoveSampleCommand(id, r.ToLocationId, r.Reason, r.Remarks), cancellationToken));

    [HttpPost("{id:guid}/tests")]
    [Authorize(Roles = SampleTrackingRoles.QcUser)]
    public async Task<IActionResult> RecordTest(Guid id, [FromBody] RecordTestRequest r, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new RecordTestCommand(
            id, r.TestingType, r.TestingStandard, r.TestingDate, r.Outcome, r.Remarks,
            r.DefectDescription, r.PhotoEvidenceRef, r.ReportAttachmentRef), cancellationToken));

    [HttpPost("{id:guid}/disposal")]
    [Authorize(Roles = SampleTrackingRoles.QcUser)]
    public async Task<IActionResult> RequestDisposal(Guid id, [FromBody] RequestDisposalRequest r, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new RequestDisposalCommand(id, r.Reason, r.Remarks), cancellationToken));

    [HttpPost("{id:guid}/disposal/approve")]
    [Authorize(Roles = SampleTrackingRoles.Admin)]
    public async Task<IActionResult> ApproveDisposal(Guid id, [FromBody] ApproveDisposalRequest r, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new ApproveDisposalCommand(id, r.DisposalDate, r.Remarks), cancellationToken));

    [HttpPost("{id:guid}/disposal/reject")]
    [Authorize(Roles = SampleTrackingRoles.Admin)]
    public async Task<IActionResult> RejectDisposal(Guid id, [FromBody] RejectDisposalRequest r, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new RejectDisposalCommand(id, r.Remarks), cancellationToken));

    [HttpPost("{id:guid}/attachments")]
    [Authorize(Roles = SampleTrackingRoles.QcUser)]
    [RequestSizeLimit(26_214_400)] // 25 MB
    public async Task<IActionResult> UploadAttachment(
        Guid id,
        [FromForm] IFormFile file,
        [FromForm] string category,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "A non-empty file is required." });

        var key = $"{id}/{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
        await using var stream = file.OpenReadStream();
        var fileRef = await _fileStorage.UploadAsync(
            AttachmentBucket, key, stream, file.ContentType ?? "application/octet-stream", cancellationToken);

        return RespondCreated(await Sender.Send(new AddSampleAttachmentCommand(
            id, file.FileName, fileRef, file.ContentType, file.Length, category), cancellationToken));
    }

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    [Authorize(Roles = SampleTrackingRoles.QcUser)]
    public async Task<IActionResult> RemoveAttachment(Guid id, Guid attachmentId, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(new RemoveSampleAttachmentCommand(id, attachmentId), cancellationToken));

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
    public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSampleDetailQuery(id), cancellationToken);
        if (!result.IsSuccess)
            return Problem(result.Error);

        var attachment = result.Value.Attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment is null)
            return NotFound(new { error = "Attachment not found." });

        var stream = await _fileStorage.DownloadAsync(AttachmentBucket, attachment.FileRef, cancellationToken);
        return File(stream, attachment.ContentType ?? "application/octet-stream", attachment.FileName);
    }
}

public sealed record RegisterSampleRequest(
    Guid RackLocationId, string SampleName, string? ItemCode, string? ProductCategory, string? Model,
    string? Size, string? Variant, string? MaterialType, string? SupplierName, string? SupplierCode,
    string? PurchaseOrderNumber, string? GrnNumber, string? SupplierReference, DateOnly ReceivedDate,
    TimeOnly? ReceivedTime, decimal ReceivedQuantity, string? ReceivedBy, string? Department, string? SampleOwner,
    string? Description, string? Purpose, string? TestingRequirement, string? RequiredTestingType, string? StorageRemarks);

public sealed record UpdateSampleRequest(
    string SampleName, string? ItemCode, string? ProductCategory, string? Model, string? Size, string? Variant,
    string? MaterialType, string? SupplierName, string? SupplierCode, string? PurchaseOrderNumber, string? GrnNumber,
    string? SupplierReference, DateOnly ReceivedDate, TimeOnly? ReceivedTime, decimal ReceivedQuantity,
    string? ReceivedBy, string? Department, string? SampleOwner, string? Description, string? Purpose,
    string? TestingRequirement, string? RequiredTestingType, string? StorageRemarks);

public sealed record ChangeStatusRequest(string Status, string? Reason, string? Remarks);
public sealed record MoveSampleRequest(Guid ToLocationId, string? Reason, string? Remarks);
public sealed record RecordTestRequest(
    string TestingType, string? TestingStandard, DateTime TestingDate, string Outcome, string? Remarks,
    string? DefectDescription, string? PhotoEvidenceRef, string? ReportAttachmentRef);
public sealed record RequestDisposalRequest(string Reason, string? Remarks);
public sealed record ApproveDisposalRequest(DateOnly? DisposalDate, string? Remarks);
public sealed record RejectDisposalRequest(string? Remarks);
