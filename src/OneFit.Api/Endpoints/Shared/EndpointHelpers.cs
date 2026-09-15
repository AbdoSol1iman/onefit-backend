using OneFit.Application.Common.Exceptions;

namespace OneFit.Api.Endpoints.Shared;

internal static class EndpointHelpers
{
    internal static IResult BadRequest(string code, string message) =>
        Results.BadRequest(new { error = new { code, message } });

    internal static IResult FromNotFoundException(NotFoundException ex) =>
        ex.ErrorCode == "INVALID_REQUEST"
            ? BadRequest(ex.ErrorCode, ex.Message)
            : Results.NotFound(new { error = new { code = ex.ErrorCode, message = ex.Message } });

    internal static IResult InternalError() =>
        Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error");

    internal static IResult? Require(string? value, string field, string? headerName = null) =>
        string.IsNullOrWhiteSpace(value)
            ? BadRequest("INVALID_REQUEST", headerName is null ? $"{field} is required." : $"{headerName} header is required.")
            : null;
}
