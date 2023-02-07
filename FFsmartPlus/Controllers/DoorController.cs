using FFsmartPlus.Services;
using Infrastructure;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FFsmartPlus.Controllers;


[Route("api/[controller]")]
[ApiController]
public class DoorController : ControllerBase
{
    private readonly IDoorService _doorService;
    private readonly FridgeAppContext _context;

    public DoorController(FridgeAppContext context, IDoorService doorService)
    {
        _context = context;
        _doorService = doorService;
    }
/// <summary>
/// Generates a door code for a supplier
/// </summary>

    [HttpGet("GenerateCode/{id}")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.HeadChef},{UserRoles.Delivery}")]
    public async Task<ActionResult<long>> GenerateCode(long id)
{
    var supplier = await _context.Suppliers.FindAsync(id);
    if (supplier is null)
    {
        return NotFound("Supplier not found");
    }
    var code = await _doorService.GenerateNewCode(id);
    return Ok(code);
}
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.HeadChef},{UserRoles.Delivery}")]
[HttpGet("Open")]
public async Task<ActionResult<bool>> OpenDoor(long code)
{
    return  await _doorService.OpenDoorByCode(code);
}
}