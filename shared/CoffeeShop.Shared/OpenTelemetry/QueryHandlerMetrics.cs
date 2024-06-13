using System.Diagnostics.Metrics;

namespace CoffeeShop.Shared.OpenTelemetry;

public class QueryHandlerMetrics : IDisposable
{
	private readonly TimeProvider _timeProvider;
	private readonly Meter _meter;
	private readonly UpDownCounter<long> _activeEventHandlingCounter;
	private readonly Counter<long> _totalCommandsNumber;
	private readonly Histogram<double> _eventHandlingDuration;

	public QueryHandlerMetrics(
		IMeterFactory meterFactory,
		TimeProvider timeProvider
	)
	{
		_timeProvider = timeProvider;
		_meter = meterFactory.Create(ActivitySourceProvider.DefaultSourceName);

		_totalCommandsNumber = _meter.CreateCounter<long>(
			TelemetryTags.Queries.TotalQueriesNumber,
			unit: "{query}",
			description: "Total number of queries send to query handlers");

		_activeEventHandlingCounter = _meter.CreateUpDownCounter<long>(
			TelemetryTags.Queries.ActiveQueriesNumber,
			unit: "{query}",
			description: "Number of queries currently being handled");

		_eventHandlingDuration = _meter.CreateHistogram<double>(
			TelemetryTags.Queries.QueryHandlingDuration,
			unit: "s",
			description: "Measures the duration of inbound queries");
	}

	public long QueryHandlingStart(string queryType)
	{
		var tags = new TagList { { TelemetryTags.Queries.QueryType, queryType } };

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

	public void QueryHandlingEnd(string queryType, long startingTimestamp)
	{
		var tags = _activeEventHandlingCounter.Enabled
				   || _eventHandlingDuration.Enabled
			? new TagList { { TelemetryTags.Queries.QueryType, queryType } }
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
