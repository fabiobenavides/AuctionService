using System;
using BiddingService_controllers.Models;
using Contracts;
using MassTransit;
using MassTransit.Testing;
using MongoDB.Entities;

namespace BiddingService_controllers.Services;

public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _services;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger,
        IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting check for finished auction");

        stoppingToken.Register(() => _logger.LogInformation("==> Auction check is stopping"));

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctions(stoppingToken);

            await Task.Delay(5000, stoppingToken);
        }

    }

    private async Task CheckAuctions(CancellationToken stoppingToken)
    {
        var finishedAuctions = await DB.Find<Auction>()
            .Match(x => x.AuctionEnd < DateTime.UtcNow)
            .Match(x => !x.Finished)
            .ExecuteAsync(stoppingToken);

        if (finishedAuctions.Count == 0) return;

        _logger.LogInformation("==> Found {count} auctions that have completed", finishedAuctions.Count);

        using var scope = _services.CreateScope();
        var endPoint = scope.ServiceProvider.GetService<IPublishEndpoint>();

        foreach (var auction in finishedAuctions)
        {
            auction.Finished = true;
            await auction.SaveAsync(null, stoppingToken);

            var winningBid = await DB.Find<Bid>()
                .Match(w => w.AuctionId == auction.ID)
                .Match(w => w.BidStatus == BidStatus.Accepted)
                .Sort(w => w.Descending(a => a.Amount))
                .ExecuteFirstAsync(stoppingToken);

            var auctionFinished = new AuctionFinished
            {
                AuctionId = auction.ID,
                Seller = auction.Seller
            };

            if (winningBid != null)
            {
                auctionFinished.Amount = winningBid.Amount;
                auctionFinished.ItemSold = true;
                auctionFinished.Winner = winningBid.Bidder;
            }

            await endPoint.Publish(auctionFinished, stoppingToken);

        }


    }
}
