using AuctionService.Dtos;

namespace AuctionService.Data;

public interface IAuctionRepository
{
    Task<List<AuctionDto>> GetAuctionsAsync(string date);
    Task<AuctionDto> GetAuctionByIdAsync(Guid id);
    Task<AuctionDto> GetAuctionByDataAsync(AuctionDto auctionDto);
    void Add(CreateAuctionDto createAuctionDto);
    Task<bool> UpdateAsync(Guid id, UpdateAuctionDto auctionDto);
    void Remove(AuctionDto auctionDto);
    Task<bool> SaveChangesAsync();
}
