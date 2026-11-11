using SampleRack.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers.V1.SampleTracking;

/// <summary>
/// Role groups for the Sample Racking module, mapped onto the seeded system roles:
/// Admin  = Administrator / QC Manager (manage racks, settings, master data, approve disposal)
/// QcUser = the above plus QC Technician (register, test, move samples)
/// Read   = any authenticated user, including Viewer.
/// </summary>
public static class SampleTrackingRoles
{
    public const string Admin = "Administrator,QC Manager";

    public const string QcUser = "Administrator,QC Manager,QC Technician";
}

/// <summary>
/// Base controller translating the Result envelope to appropriate HTTP responses.
/// </summary>
public abstract class SampleTrackingControllerBase : ControllerBase
{
    protected readonly ISender Sender;

    protected SampleTrackingControllerBase(ISender sender)
    {
        Sender = sender;
    }

    protected IActionResult Respond<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : Problem(result.Error);

    protected IActionResult RespondCreated<T>(Result<T> result, string? location = null)
        => result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : Problem(result.Error);

    protected IActionResult RespondNoContent(Result result)
        => result.IsSuccess ? NoContent() : Problem(result.Error);

    protected IActionResult Problem(Error error)
    {
        var status = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };
        return StatusCode(status, new { error = error.Message, code = error.Code });
    }
}
