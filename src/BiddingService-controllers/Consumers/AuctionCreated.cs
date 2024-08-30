using System;
using BiddingService_controllers.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService_controllers.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
       var auction = new Auction
       {
            ID = context.Message.Id.ToString(),
            Seller = context.Message.Seller,
            AuctionEnd = context.Message.AuctionEnd,
            ReservedPrice = context.Message.ReservePrice
       };

       await auction.SaveAsync();
    }
}
