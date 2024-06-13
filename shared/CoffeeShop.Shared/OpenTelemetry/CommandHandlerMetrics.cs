using System.Diagnostics.Metrics;

namespace CoffeeShop.Shared.OpenTelemetry;

public class CommandHandlerMetrics : IDisposable
{
	private readonly TimeProvider _timeProvider;
	private readonly Meter _meter;
	private readonly UpDownCounter<long> _activeEventHandlingCounter;
	private readonly Counter<long> _totalCommandsNumber;
	private readonly Histogram<double> _eventHandlingDuration;

	public CommandHandlerMetrics(
		IMeterFactory meterFactory,
		TimeProvider timeProvider
	)
	{
		_timeProvider = timeProvider;
		_meter = meterFactory.Create(ActivitySourceProvider.DefaultSourceName);

		_totalCommandsNumber = _meter.CreateCounter<long>(
			TelemetryTags.Commands.TotalCommandsNumber,
			unit: "{command}",
			description: "Total number of commands send to command handlers");

		_activeEventHandlingCounter = _meter.CreateUpDownCounter<long>(
			TelemetryTags.Commands.ActiveCommandsNumber,
			unit: "{command}",
			description: "Number of commands currently being handled");

		_eventHandlingDuration = _meter.CreateHistogram<double>(
			TelemetryTags.Commands.CommandHandlingDuration,
			unit: "s",
			description: "Measures the duration of inbound commands");
	}

	public long CommandHandlingStart(string commandType)
	{
		var tags = new TagList { { TelemetryTags.Commands.CommandType, commandType } };

		if (_activeEventHandlingCounter.Enabled)
		{
			_activeEventHandlingCounter.Add(1, tags);
		}

		if (_totalCommandsNumber.Enabled)
		{
			_totalCommandsNumber.Add(1, tags);
		}

		return _timeProvider.GetTimestamp();
	}

	public void CommandHandlingEnd(string commandType, long startingTimestamp)
	{
		var tags = _activeEventHandlingCounter.Enabled
				   || _eventHandlingDuration.Enabled
			? new TagList { { TelemetryTags.Commands.CommandType, commandType } }
			: default;

		if (_activeEventHandlingCounter.Enabled)
		{
			_activeEventHandlingCounter.Add(-1, tags);
		}

		if (!_eventHandlingDuration.Enabled) return;

		var elapsed = _timeProvider.GetElapsedTime(startingTimestamp);

		_eventHandlingDuration.Record(
			elapsed.TotalSeconds,
			tags);
	}
	public void Dispose() => _meter.Dispose();
}
