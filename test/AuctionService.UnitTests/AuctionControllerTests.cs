using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _mockRepository;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionController _sut;
    private readonly IMapper _mapper;

    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _mockRepository = new Mock<IAuctionRepository>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);
        _sut = new AuctionController(_mockRepository.Object, _mapper, _mockPublishEndpoint.Object);

    }

    [Fact]
    public async void GetAuctionById_Should_return_Auction()
    {
        // Arrange
        var auction = _fixture.Create<AuctionDto>();
        _mockRepository
            .Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auction);

        // Act
        var result = await _sut.GetAuctionById(auction.Id);

        // Assert
        Assert.Equal(auction, result.Value);
        Assert.IsType<ActionResult<AuctionDto>>(result);
    }

    [Fact]
    public async void GetAuctionById_Should_return_NotFound()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);

        // Act
        var result = await _sut.GetAuctionById(Guid.Empty);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async void CreateAuction_Should_return_CreatedAtAction()
    {
        //Arrange 
        var createdAuctionDto = _fixture.Create<CreateAuctionDto>();

        _mockRepository
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        
        var auctionDto = new AuctionDto
        {
            Id = Guid.NewGuid(),
            Make = createdAuctionDto.Make,
            Model = createdAuctionDto.Model,
            Year = createdAuctionDto.Year,
            Color = createdAuctionDto.Color,
            Mileage = createdAuctionDto.Mileage,
            ImageUrl = createdAuctionDto.ImageUrl,
            ReservePrice = createdAuctionDto.ReservePrice,
            AuctionEnd = createdAuctionDto.AuctionEnd,
            Seller = createdAuctionDto.Seller
        };
            
        _mockRepository
            .Setup(r => r.GetAuctionByDataAsync(auctionDto))
            .ReturnsAsync(auctionDto);

        // Act
        var result = await _sut.CreateAuction(createdAuctionDto);
        var CreatedResult = result.Result as CreatedAtActionResult;
        // Assert
        Assert.NotNull(CreatedResult);
        Assert.Equal("GetAuctionById", CreatedResult.ActionName);
        Assert.IsType<AuctionDto>(CreatedResult.Value);
    }
}
