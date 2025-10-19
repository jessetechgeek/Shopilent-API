using System.Net;
using System.Text.Json;
using Shopilent.API.IntegrationTests.Common;

namespace Shopilent.API.IntegrationTests.Infrastructure;

public class HealthCheckTests : ApiIntegrationTestBase
{
    public HealthCheckTests(ApiIntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task HealthCheck_Should_Return_Healthy_Status()
    {
        // Act
        var response = await Client.GetAsync("../health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        // Parse the health check response
        var healthCheckResponse = JsonSerializer.Deserialize<HealthCheckResponse>(content, JsonOptions);

        healthCheckResponse.Should().NotBeNull();
        healthCheckResponse!.Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthCheck_Should_Include_Database_Check()
    {
        // Act
        var response = await Client.GetAsync("../health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var healthCheckResponse = JsonSerializer.Deserialize<HealthCheckResponse>(content, JsonOptions);

        healthCheckResponse.Should().NotBeNull();
        healthCheckResponse!.Entries.Should().ContainKey("postgresql-main");
        healthCheckResponse.Entries["postgresql-main"].Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthCheck_Should_Include_Redis_Check()
    {
        // Act
        var response = await Client.GetAsync("../health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var healthCheckResponse = JsonSerializer.Deserialize<HealthCheckResponse>(content, JsonOptions);

        healthCheckResponse.Should().NotBeNull();
        healthCheckResponse!.Entries.Should().ContainKey("redis");
        healthCheckResponse.Entries["redis"].Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthCheck_Should_Include_Meilisearch_Check()
    {
        // Act
        var response = await Client.GetAsync("../health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var healthCheckResponse = JsonSerializer.Deserialize<HealthCheckResponse>(content, JsonOptions);

        healthCheckResponse.Should().NotBeNull();
        healthCheckResponse!.Entries.Should().ContainKey("meilisearch");
        healthCheckResponse.Entries["meilisearch"].Status.Should().Be("Healthy");
    }

    private class HealthCheckResponse
    {
        public string Status { get; set; } = string.Empty;
        public Dictionary<string, HealthCheckEntry> Entries { get; set; } = new();
        public TimeSpan TotalDuration { get; set; }
    }

    private class HealthCheckEntry
    {
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TimeSpan Duration { get; set; }
        public Dictionary<string, object>? Data { get; set; }
        public List<string>? Tags { get; set; }
    }
}
