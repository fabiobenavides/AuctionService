using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionRepository : IAuctionRepository
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionRepository(AuctionDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public void Add(CreateAuctionDto createAuctionDto)
    {
        var auction = _mapper.Map<Auction>(createAuctionDto);
        _context.Add(auction);
    }

    public Task<List<AuctionDto>> GetAuctionsAsync(string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query
                .Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }
        return query
            .ProjectTo<AuctionDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<AuctionDto> GetAuctionByIdAsync(Guid id)
    {
        return await _context.Auctions
            .ProjectTo<AuctionDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    
    public void Remove(AuctionDto auctionDto)
    {
        var auctionEntity = _mapper.Map<Auction>(auctionDto);
        _context.Auctions.Remove(auctionEntity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateAuctionDto updateAuctionDto)
    {
         var auctionEntity = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auctionEntity == null)
            return false;

        auctionEntity.Item.Make = updateAuctionDto.Make ?? auctionEntity.Item.Make;
        auctionEntity.Item.Model = updateAuctionDto.Model ?? auctionEntity.Item.Model;
        auctionEntity.Item.Year = updateAuctionDto.Year ?? auctionEntity.Item.Year;
        auctionEntity.Item.Color = updateAuctionDto.Color ?? auctionEntity.Item.Color;
        auctionEntity.Item.Mileage = updateAuctionDto.Mileage ?? auctionEntity.Item.Mileage;

        return await SaveChangesAsync();
    }

    public async Task<AuctionDto> GetAuctionByDataAsync(AuctionDto auctionDto)
    {
        var query = await _context.Auctions
            .FirstOrDefaultAsync(x => x.ReservePrice == auctionDto.ReservePrice
                && x.Seller == auctionDto.Seller
                && x.Winner == auctionDto.Winner
                && x.SoldAmount == auctionDto.SoldAmount
                && x.CurrentHighBid == auctionDto.CurrentHighBid
                && x.Item.Make == auctionDto.Make
                && x.Item.Model == auctionDto.Model
                && x.Item.Year == auctionDto.Year
                && x.Item.Color == auctionDto.Color
                && x.Item.Mileage == auctionDto.Mileage
                && x.Item.ImageUrl == auctionDto.ImageUrl);

        return _mapper.Map<AuctionDto>(query);
    }
}
