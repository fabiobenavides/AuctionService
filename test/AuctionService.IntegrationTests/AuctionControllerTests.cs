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

[Collection("Shared collection")]
public class AuctionControllerTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private const string VEYRON_ID = "c8c3ec17-01bf-49db-82aa-1ef80b833a9f";
    private const string FORD_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

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

    [Fact]
    public async Task CreateAuction_WithNoAuth_ShouldReturnUnauthorized()
    {
        // Arrange ?
        var auction = new CreateAuctionDto{Make="test"};
        // Act
        var result = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithAuth_ShouldReturnCreated()
    {
        // Arrange ?
        var auction = GetAuctionDtoForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // Act
        var result = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        // Assert
        result.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        var createdAuction = await result.Content.ReadFromJsonAsync<AuctionDto>();
        Assert.Equal("bob", createdAuction.Seller);
    }

    [Fact]
    public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
    {
        // Arrange ?
        var auction = GetAuctionDtoForCreate();
        auction.Make = null;
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // Act
        var result = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
    {
        // arrange
        var updateAuctionDto = GetAuctionDtoForUpdate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("Fab"));

        // act
        var result = await _httpClient.PutAsJsonAsync($"api/auctions/{FORD_ID}", updateAuctionDto);

        // assert
        result.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
    {
        // arrange
        var updateAuctionDto = GetAuctionDtoForUpdate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("Bob"));

        // act
        var result = await _httpClient.PutAsJsonAsync($"api/auctions/{FORD_ID}", updateAuctionDto);

        // assert
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
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

    private CreateAuctionDto GetAuctionDtoForCreate()
    {
        return new CreateAuctionDto
        {
            Make = "test",
            Model = "test Model",
            ImageUrl = "testImageUrl",
            Color = "testColor",
            Mileage = 10,
            Year = 10,
            ReservePrice = 10
        };
    }

    private UpdateAuctionDto GetAuctionDtoForUpdate()
    {
        return new UpdateAuctionDto
        {
            Make = "Ford Updated",
            Model = "GT Updated",
            Color = "White Updated",
            Mileage = 55,
            Year = 2024,
        };
    }
}
