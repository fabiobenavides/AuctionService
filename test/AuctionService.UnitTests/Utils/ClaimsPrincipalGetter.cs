using System.Security.Claims;

namespace AuctionService.UnitTests.Utils;

public class ClaimsPrincipalGetter
{
    public static ClaimsPrincipal GetPrincipal()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "test")
        };
        var identity = new ClaimsIdentity(claims, "testing");
        return new ClaimsPrincipal(identity);
    }
}
