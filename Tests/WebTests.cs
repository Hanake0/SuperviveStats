using Aspire.Hosting;

namespace Tests;

public class WebTests {
	[Fact]
	public async Task GetWebResourceRootReturnsOkStatusCode() {
		// Arrange
		IDistributedApplicationTestingBuilder appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Host>();
		appHost.Services.ConfigureHttpClientDefaults(clientBuilder => {
			clientBuilder.AddStandardResilienceHandler();
		});

		await using DistributedApplication app                         = await appHost.BuildAsync();
		ResourceNotificationService        resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
		await app.StartAsync();

		// Act
		HttpClient httpClient = app.CreateHttpClient("frontend");
		await resourceNotificationService.WaitForResourceAsync("frontend", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
		HttpResponseMessage response = await httpClient.GetAsync("/");

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}
}