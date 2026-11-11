using SampleRack.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers;

/// <summary>Role groups used by [Authorize(Roles = ...)] across the API.</summary>
public static class ApiRoles
{
    public const string Admin = "Administrator,QC Manager";
    public const string AdminOnly = "Administrator";
}

/// <summary>
/// Base controller translating the Result envelope into appropriate HTTP responses.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected readonly ISender Sender;

    protected ApiControllerBase(ISender sender)
    {
        Sender = sender;
    }

    protected IActionResult Respond<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : Problem(result.Error);

    protected IActionResult RespondCreated<T>(Result<T> result)
        => result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result.Value) : Problem(result.Error);

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
