using System.Security.Claims;
using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        }).CreateMapper();

        _mapper = mockMapper;

        _sut = new AuctionController(_mockRepository.Object, _mapper, _mockPublishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = ClaimsPrincipalGetter.GetPrincipal()
                }
            }
        };

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
        
        var auctionDto = _mapper.Map<AuctionDto>(createdAuctionDto);
            
        _mockRepository
            .Setup(r => r.GetAuctionByDataAsync(It.IsAny<AuctionDto>()))
            .ReturnsAsync(auctionDto);

        // Act
        var result = await _sut.CreateAuction(createdAuctionDto);
        var CreatedResult = result.Result as CreatedAtActionResult;
        // Assert
        Assert.NotNull(CreatedResult);
        Assert.Equal("GetAuctionById", CreatedResult.ActionName);
        Assert.IsType<AuctionDto>(CreatedResult.Value);
    }

    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        //Arrange 
        var createdAuctionDto = _fixture.Create<CreateAuctionDto>();

        _mockRepository
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(false);
        
        // Act
        var result = await _sut.CreateAuction(createdAuctionDto);
        var CreatedResult = result.Result as CreatedAtActionResult;
        // Assert
        Assert.Null(CreatedResult);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        //Arrange 
        var updatedAuctionDto = _fixture.Create<UpdateAuctionDto>();
               
        var auctionDto = _fixture.Create<AuctionDto>();

        auctionDto.Seller = ClaimsPrincipalGetter
            .GetPrincipal()
            .Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Name)
            .Value;
            
        _mockRepository
            .Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auctionDto);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Guid>(), updatedAuctionDto))
            .ReturnsAsync(true);

        _mockPublishEndpoint.Setup(r => r.Publish(updatedAuctionDto, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true));

        // Act
        var result = await _sut.Update(new Guid(), updatedAuctionDto);
        // Assert
        Assert.NotNull(result);
        Assert.IsType<OkResult>(result);   
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        //Arrange 
        var updatedAuctionDto = _fixture.Create<UpdateAuctionDto>();
               
        var auctionDto = _fixture.Create<AuctionDto>();
            
        _mockRepository
            .Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auctionDto);

        // Act
        var result = await _sut.Update(new Guid(), updatedAuctionDto);
        // Assert
        Assert.IsType<ForbidResult>(result);  
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
         //Arrange 
        var updatedAuctionDto = _fixture.Create<UpdateAuctionDto>();
               
        AuctionDto auctionDto = null;
            
        _mockRepository
            .Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auctionDto);

        // Act
        var result = await _sut.Update(new Guid(), updatedAuctionDto);
        // Assert
        Assert.IsType<NotFoundResult>(result);  
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        //Arrange 
        var auctionDto = _fixture.Create<AuctionDto>();

        _mockRepository
            .Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auctionDto);

        auctionDto.Seller = ClaimsPrincipalGetter
            .GetPrincipal()
            .Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Name)
            .Value;
        
        var auctionDeleted = new AuctionDeleted
        {
            Id = auctionDto.Id.ToString()
        };

        _mockPublishEndpoint.Setup(r => r.Publish(auctionDeleted, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true));

        _mockRepository
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Delete(auctionDto.Id);
        // Assert
        Assert.NotNull(result);
        Assert.IsType<OkResult>(result);   
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        //Arrange 
        AuctionDto auctionDto = null;

        _mockRepository
            .Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auctionDto);

        // Act
        var result = await _sut.Delete(new Guid());
        // Assert
        Assert.IsType<NotFoundResult>(result);  
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns403Response()
    {
        //Arrange 
        var auctionDto = _fixture.Create<AuctionDto>();

        _mockRepository
            .Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auctionDto);

        // Act
        var result = await _sut.Delete(auctionDto.Id);
        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns400BadRequest()
    {
        //Arrange 
        var auctionDto = _fixture.Create<AuctionDto>();

        _mockRepository
            .Setup(r => r.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auctionDto);

        auctionDto.Seller = ClaimsPrincipalGetter
            .GetPrincipal()
            .Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Name)
            .Value;
        
        var auctionDeleted = new AuctionDeleted
        {
            Id = auctionDto.Id.ToString()
        };

        _mockPublishEndpoint.Setup(r => r.Publish(auctionDeleted, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true));

        _mockRepository
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Delete(auctionDto.Id);
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);  
    }
}
