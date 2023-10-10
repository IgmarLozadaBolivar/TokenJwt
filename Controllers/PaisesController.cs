using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace TokenJwt.Controllers;

[Route("api/[controller]")]
public class PaisesController : ControllerBase
{
    [Authorize]    // * Anotacion que indica que se requiere autenticar
    [HttpGet]    // * Metodo HttpGet
    [Route("Lista")]    // * Especifica la ruta
    public async Task<IActionResult> Lista()
    {
        // * Lista de nombres de paises 
        var listaPaises = await Task.FromResult(new List<string> { "Francia", "Argentina", "Croacia", "Marruecos" });
        // * Retorna una respuesta Ok con la lista de paises
        return Ok(listaPaises);
    }
}