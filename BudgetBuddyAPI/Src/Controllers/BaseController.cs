using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetBuddyAPI;

public abstract class BaseController : ControllerBase
{
    protected Guid UserId
    {
        get
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(id, out var guid))
                throw new UnauthorizedAccessException("Invalid user ID");

            return guid;
        }
    }
}