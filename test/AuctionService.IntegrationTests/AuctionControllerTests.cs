using System;
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

public class AuctionControllerTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private const string VEYRON_ID = "c8c3ec17-01bf-49db-82aa-1ef80b833a9f";

    public AuctionControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ShouldReturn3Auctions()
    {
        // Arrange ?

        // Act
        var result = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
    {
        // Arrange ?

        // Act
        var result = await _httpClient.GetFromJsonAsync<AuctionDto>("api/auctions/" + VEYRON_ID);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Bugatti", result.Make);
        Assert.Equal("Veyron", result.Model);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange ?

        // Act
        var result = await _httpClient.GetAsync("api/auctions/" + Guid.NewGuid());

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ShouldReturnBadrequest()
    {
        // Arrange ?

        // Act
        var result = await _httpClient.GetAsync("api/auctions/notAGuid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReinitDbForTests(db);
        return Task.CompletedTask;
    }
}
