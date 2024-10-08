using AuctionService.Data;
using AuctionService.Dtos;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionController(IAuctionRepository auctionRepository,
        IMapper mapper,
        IPublishEndpoint publishEndpoint)
    {
        _auctionRepository = auctionRepository;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        return await _auctionRepository
            .GetAuctionsAsync(date);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _auctionRepository
            .GetAuctionByIdAsync(id);
        
        if (auction == null)
        {
            return NotFound();
        }

        return auction;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
    {

        createAuctionDto.Seller = User.Identity.Name;

        _auctionRepository.Add(createAuctionDto);

        var newAuctionDto = _mapper.Map<AuctionDto>(createAuctionDto);

        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuctionDto));

        var result = await _auctionRepository.SaveChangesAsync();

        if (!result)
            return BadRequest("Could not save changes.");

        var auctionCreated = await _auctionRepository.GetAuctionByDataAsync(newAuctionDto);

        if (auctionCreated == null)
            return BadRequest("Could not save changes.");

        return CreatedAtAction(nameof(GetAuctionById), new { auctionCreated.Id }, newAuctionDto);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> Update(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _auctionRepository.GetAuctionByIdAsync(id);

        if (auction == null)
            return NotFound();
        
        if (User.Identity.Name != auction.Seller)
        {
            return Forbid(); 
        }
        
        var results = await _auctionRepository.UpdateAsync(id, updateAuctionDto);

        if (results)
        {
            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
            return Ok();
        }

        return BadRequest("Problem saving");
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var auction = await _auctionRepository.GetAuctionByIdAsync(id);

        if (auction == null)
            return NotFound();
        
        if (User.Identity.Name != auction.Seller)
        {
            return Forbid(); 
        }

        var AuctionDeleted = new AuctionDeleted
        {
            Id = id.ToString()
        };

        await _publishEndpoint.Publish(AuctionDeleted);

        _auctionRepository.Remove(auction);

        var results = await _auctionRepository.SaveChangesAsync();

        if (results)
            return Ok();

        return BadRequest("Problem saving");

    }
}
