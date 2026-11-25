using Claims.Api.Application;
using Claims.Api.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Claims.Api.Controllers;

/// <summary>Turns known application and domain exceptions into ProblemDetails responses. Anything else stays a 500.</summary>
public sealed class ApiExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch
        {
            ValidationException validation => new ValidationProblemDetails(
                validation.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
            },
            NotFoundException => Create(StatusCodes.Status404NotFound, "Not found", exception.Message),
            NotPermittedException => Create(StatusCodes.Status403Forbidden, "Not permitted", exception.Message),
            InvalidTransitionException => Create(StatusCodes.Status409Conflict, "Invalid state change", exception.Message),
            DbUpdateConcurrencyException => Create(
                StatusCodes.Status409Conflict,
                "Claim changed",
                "Someone else changed this claim at the same time. Reload it and try again."),
            _ => null,
        };

        if (problem is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static ProblemDetails Create(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };
}
