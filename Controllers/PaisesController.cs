using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace TokenJwt.Controllers;

[Route("api/[controller]")]
public class PaisesController : ControllerBase
{
    [Authorize]
    [HttpGet]
    [Route("Lista")]
    public async Task<IActionResult> Lista()
    {
        var listaPaises = await Task.FromResult(new List<string> { "Francia", "Argentina", "Croacia", "Marruecos" });
        return Ok(listaPaises);
    }
}