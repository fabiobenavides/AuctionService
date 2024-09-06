using System;
using AuctionService;
using BiddingService_controllers.Models;
using Grpc.Net.Client;

namespace BiddingService_controllers.Services;

public class GrpcAuctionClient
{
    private readonly ILogger<GrpcAuctionClient> _logger;
    private readonly IConfiguration _config;

    public GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;       
    }

    public Auction GetAuction(string id)
    {
        _logger.LogInformation("Calling GRPC service");
        var channel = GrpcChannel.ForAddress(_config["GrpcAution"]);
        var client = new GrpcAuction.GrpcAuctionClient(channel);
        var request = new GetAuctionRequest {
            Id = id
        };

        try
        {
            var reply = client.GetAuction(request);
            var auction = new Auction 
            {
                ID = reply.Auction.Id,
                AuctionEnd = DateTime.Parse(reply.Auction.AuctionEnd),
                Seller = reply.Auction.Seller,
                ReservedPrice = reply.Auction.ReservedPrice
            };

            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not call GRPC Server");
            return null;
        }

    }

}
