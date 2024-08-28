using BiddingService_controllers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService_controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidsController : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Bid>> PlaceBig(string auctionId, int amount)
        {
            var auction = await DB.Find<Auction>().OneAsync(auctionId);
            if (auction == null)
            {
                // Todo: check with auction service it that has auction
                return NotFound();
            }
            if (auction.Seller == User.Identity.Name)
            {
                return BadRequest("You cannot bid on your own auction");
            }
            var bid = new Bid
        }
    }
}
