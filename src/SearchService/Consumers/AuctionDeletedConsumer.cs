using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine("-----> Consuming auction deleted: " + context.Message.Id);

        await DB.DeleteAsync<Item>(context.Message.Id);

    }
}
