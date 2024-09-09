using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddReverseProxy()
				.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
				.AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.MapReverseProxy();

// todo: only for testing purposes, remove later
// https://stackoverflow.com/questions/70306118/set-traceid-on-activity
app.Use((context, next) =>
{
	var traceIdFromProxy = context.Request.Headers["trace-id"].FirstOrDefault();
	if (traceIdFromProxy != null)
	{
		traceIdFromProxy = traceIdFromProxy.Replace("-", "");
	}

	if (context.Request.Headers.TryGetValue("traceparent", out var traceParent))
	{
		if (context.Request.Headers.Remove("traceparent"))
		{
			var p = traceParent.SelectMany(x => x!.Split('-')).ToArray();

			Activity.Current = new Activity("Yarp.ReverseProxy")
				.SetParentId($"{p[0]}-{traceIdFromProxy}-{p[2]}-{p[3]}")
				.Start();
		}
	}

	return next(context);
});

app.Run();
