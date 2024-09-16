using System.Diagnostics;
using System.Text.RegularExpressions;

using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddReverseProxy()
				.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
				.AddTransforms(b =>
				{
					// todo: only for testing purposes, remove later
					// https://stackoverflow.com/questions/70306118/set-traceid-on-activity
					b.AddRequestTransform(async transformContext =>
					{
						var context = transformContext.HttpContext;
						var traceIdFromProxy = context.Request.Headers["trace-id"].FirstOrDefault()?.Trim('"');
						if (Guid.TryParse(traceIdFromProxy, out var parsedGuid))
						{
							traceIdFromProxy = parsedGuid.ToString("N");
						}
						else
						{
							// Handle the case where the traceId is not a valid GUID
							traceIdFromProxy = Guid.NewGuid().ToString("N");
						}

						if (context.Request.Headers.TryGetValue("traceparent", out var traceParent))
						{
							if (context.Request.Headers.Remove("traceparent"))
							{
								var traceParentReplaced = Regex.Replace(traceParent, "-.*?-", $"-${traceIdFromProxy}-");
								Activity.Current = new Activity("Yarp.ReverseProxy")
									.SetParentId(traceParentReplaced)
									.Start();
							}
						}

						await ValueTask.CompletedTask;
					});
				})
				.AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.MapReverseProxy();

app.Run();
