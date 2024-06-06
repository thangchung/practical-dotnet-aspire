using CoffeeShop.Shared.OpenTelemetry;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoffeeShop.Shared.Exceptions;

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		if (exception is not ValidationException validationException)
		{
			return false;
		}

		logger.LogError(
			validationException, "Exception occurred: {Message}", validationException.Message);

		var problemDetails = new ProblemDetails
		{
			Status = StatusCodes.Status400BadRequest,
			Type = "ValidationFailure",
			Title = "Validation error",
			Detail = "One or more validation errors has occurred"
		};

		if (validationException.Errors is not null)
		{
			problemDetails.Extensions["errors"] = validationException.Errors;
		}

		httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

		await httpContext.Response
			.WriteAsJsonAsync(problemDetails, cancellationToken);

		return true;
	}
}
