using System;
using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
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
    public void GetAuctionById_Should_return_OK()
    {

    }

    [Fact]
    public void GetAuctionById_Should_return_NotFound()
    {

    }
}
