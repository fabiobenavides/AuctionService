using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine("---> Consuming auction finished");

        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

        if (context.Message.ItemSold)
        {
            auction.SoldAmount = context.Message.Amount.Value;
            auction.Winner = context.Message.Winner;
        }

        auction.Status = "Finished";

        await auction.SaveAsync();
    }
}
