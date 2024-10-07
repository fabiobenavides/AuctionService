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
    public void Add(AuctionDto createAuctionDto)
    {
        var auction = _mapper.Map<Auction>(createAuctionDto);
        _context.Add(auction);
    }

    public Task<List<AuctionDto>> GetAuctionAsync(string date)
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
    
    public void Remove(AuctionDto auction)
    {
        var auctionEntity = _mapper.Map<Auction>(auction);
        _context.Auctions.Remove(auctionEntity);
    }

    public async Task<bool> SaveChanges()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task UpdateAsync(AuctionDto auction)
    {
         var auctionEntity = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == auction.Id);
        
        auctionEntity.Item.Make = auction.Make;
        auctionEntity.Item.Model = auction.Model;
        auctionEntity.Item.Year = auction.Year;
        auctionEntity.Item.Color = auction.Color;
        auctionEntity.Item.Mileage = auction.Mileage;

        await SaveChanges();
    }
}
