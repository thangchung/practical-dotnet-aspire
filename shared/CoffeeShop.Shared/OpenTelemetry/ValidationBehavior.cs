using FluentValidation;

namespace CoffeeShop.Shared.OpenTelemetry;

public record ValidationError(string PropertyName, string ErrorMessage);

public class ValidationException(IEnumerable<ValidationError> errors) : Exception
{
	public IEnumerable<ValidationError> Errors => errors;
}

public class ValidationBehavior<TRequest, TResponse>(IActivityScope activityScope, IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull, IRequest<TResponse>
	where TResponse : notnull
{
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var context = new ValidationContext<TRequest>(request);

		var validationFailures = await Task.WhenAll(
			validators.Select(validator => validator.ValidateAsync(context)));

		var errors = validationFailures
			.Where(validationResult => !validationResult.IsValid)
			.SelectMany(validationResult => validationResult.Errors)
			.Select(validationFailure => new ValidationError(
				validationFailure.PropertyName,
				validationFailure.ErrorMessage))
			.ToList();

		if (errors.Count != 0)
		{
			throw new ValidationException(errors);
		}

		var queryName = typeof(TRequest).Name;
		var validatorNames = validators.Aggregate("", (c, x) => $"{x.GetType().Name}, {c}");
		var activityName = $"{queryName}-{validatorNames.Trim().TrimEnd(',')}";

		return await activityScope.Run(
			activityName,
				async (_, token) => await next(),
				new StartActivityOptions { Tags = { { TelemetryTags.Validator.Validation, queryName } } },
				cancellationToken
			);
	}
}
