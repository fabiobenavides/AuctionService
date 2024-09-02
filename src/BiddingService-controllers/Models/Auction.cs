using MongoDB.Entities;

namespace BiddingService_controllers.Models;

public class Auction : Entity
{
    public DateTime AuctionEnd { get; set; }
    public string Seller { get; set; }
    public int ReservedPrice { get; set; }
    public bool Finished { get; set; }
}
