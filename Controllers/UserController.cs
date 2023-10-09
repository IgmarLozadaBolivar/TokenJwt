using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using TokenJwt.Models;
using TokenJwt.Services;
namespace TokenJwt.Controllers;

[Route("[controller]")]
public class UserController : Controller
{
    private readonly IAutorizacionService _autorizacionService;

    public UserController(IAutorizacionService autorizacionService)
    {
        _autorizacionService = autorizacionService;
    }

    [HttpPost("autenticar")]
    public async Task<IActionResult> Autenticar([FromBody] AutorizacionRequest autorizacion)
    {
        var resultadoAutorizacion = await _autorizacionService.DevolverToken(autorizacion);
        if (resultadoAutorizacion == null)
            return Unauthorized();

        return Ok(resultadoAutorizacion);
    }

    [HttpPost("obtenerRefreshToken")]
    public async Task<IActionResult> ObtenerRefreshToken([FromBody] RefreshTokenRequest request)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenExpiradoSupuestamente = tokenHandler.ReadJwtToken(request.TokenExpirado);

        if(tokenExpiradoSupuestamente.ValidTo>DateTime.UtcNow)
            return BadRequest(new AutorizacionResponse {Result = false, Msg = "Token no ha expirado"});
    
        string idUser = tokenExpiradoSupuestamente.Claims.First(x => 
            x.Type == JwtRegisteredClaimNames.NameId
        ).Value.ToString();

        var autorizacionResponse = await _autorizacionService.DevolverTokenRefresh(request, int.Parse(idUser));

        if(autorizacionResponse.Result)
            return Ok(autorizacionResponse);
        else
            return BadRequest(autorizacionResponse);
    }
}