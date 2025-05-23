using Microsoft.Extensions.Diagnostics.HealthChecks;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using Web.Components;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry(logging => {
	logging.IncludeFormattedMessage = true;
	logging.IncludeScopes           = true;
});

builder.Services.AddOpenTelemetry()
	   .WithMetrics(metrics => {
			metrics.AddAspNetCoreInstrumentation()
				   .AddHttpClientInstrumentation()
				   .AddRuntimeInstrumentation();
		})
	   .WithTracing(tracing => {
			tracing.AddSource(builder.Environment.ApplicationName)
				   .AddAspNetCoreInstrumentation()
				   .AddHttpClientInstrumentation();
		});

bool useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

if (useOtlpExporter) builder.Services.AddOpenTelemetry().UseOtlpExporter();

builder.Services.AddServiceDiscovery();

builder.Services.ConfigureHttpClientDefaults(http => {
	http.AddStandardResilienceHandler();

	http.AddServiceDiscovery();
});

builder.AddRedisOutputCache("cache");

builder.Services.AddHealthChecks()
	   .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

builder.Services.AddRazorComponents()
	   .AddInteractiveServerComponents();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error", createScopeForErrors: true);

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.MapHealthChecks("/health");

app.Run();