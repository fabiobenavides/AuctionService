using System;
using AutoMapper;
using BiddingService_controllers.DTOs;
using BiddingService_controllers.Models;

namespace BiddingService_controllers.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Bid, BidDto>();
    }
}
