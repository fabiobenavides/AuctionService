using System;
using AuctionService.Dtos;
using AuctionService.Entities;

namespace AuctionService.Data;

public interface IAuctionRepository
{
    Task<List<AuctionDto>> GetAuctionAsync(string date);
    Task<AuctionDto> GetAuctionByIdAsync(Guid id);

    void Add(AuctionDto auction);
    void UpdateAsync(AuctionDto auction);
    void Remove(Auction auction);

    Task<bool> SaveChanges();

}
