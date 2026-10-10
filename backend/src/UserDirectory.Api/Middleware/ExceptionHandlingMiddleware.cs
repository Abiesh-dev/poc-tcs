using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UserDirectory.Domain.Exceptions;

namespace UserDirectory.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private const int ClientClosedRequestStatusCode = 499;

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation("Request {Path} was cancelled by the client", context.Request.Path);
            context.Response.StatusCode = ClientClosedRequestStatusCode;
        }
        catch (Exception exception)
        {
            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogError(exception, "Response already started; unable to write error payload");
            throw exception;
        }

        var problemDetails = Map(exception);
        problemDetails.Instance = context.Request.Path;
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Path}", context.Request.Path);

            if (_environment.IsDevelopment())
            {
                problemDetails.Extensions["exception"] = exception.ToString();
            }
        }
        else
        {
            _logger.LogWarning(
                "Request {Path} rejected with {StatusCode}: {Detail}",
                context.Request.Path,
                problemDetails.Status,
                problemDetails.Detail);
        }

        context.Response.Clear();
        context.Response.StatusCode = problemDetails.Status!.Value;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails, context.RequestAborted);
    }

    private static ProblemDetails Map(Exception exception) => exception switch
    {
        UserNotFoundException notFound => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "User not found",
            Detail = notFound.Message,
        },
        DuplicateUserException duplicate => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Duplicate user",
            Detail = duplicate.Message,
        },
        InvalidUserDataException invalid => new ValidationProblemDetails(
            new Dictionary<string, string[]> { [invalid.Field] = [invalid.Message] })
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
        },
        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred",
            Detail = "The request could not be completed. Please try again later.",
        },
    };
}
