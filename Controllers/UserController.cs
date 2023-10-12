using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using TokenJwt.Models;
using TokenJwt.Services;
namespace TokenJwt.Controllers;

[Route("[controller]")]
public class UserController : Controller
{
    // * Propiedades de solo lectura con parametros
    private readonly DbAppContext _context;
    private readonly IAutorizacionService _autorizacionService;

    // * Inyeccion de dependencias necesarias de las propiedades de lectura
    public UserController(IAutorizacionService autorizacionService, DbAppContext context)
    {
        _autorizacionService = autorizacionService;
        _context = context;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> RegistrarUsuario([FromBody] RegisterRequest request)
    {
        // * Verifica si el modelo recibido en la solicitud es valido
        if (!ModelState.IsValid)
        {
            //* Si el modelo no es valido, un BadRequest
            return BadRequest(ModelState);
        }

        // * Llama el servicio para registrar un nuevo usuario
        var resultadoRegistro = await _autorizacionService.Registrar(request);

        // * Verifica si el registro fue exitoso
        if (resultadoRegistro.Result)
        {
            // * Si el registro es exitoso, un Ok
            return Ok(resultadoRegistro);
        }
        else
        {
            // * Si el registro falla, un BadRequest
            return BadRequest(resultadoRegistro);
        }
    }

    [HttpPost("validarUser")]
    public async Task<IActionResult> AutenticarUser([FromBody] AutorizacionRequest autorizacion)
    {
        // * Llama al servicio para intentar autenticar al usuario
        var usuarioEncontrado = await _autorizacionService.VerificarUsuario(autorizacion);

        // * Verifica si el usuario existe
        if (usuarioEncontrado)
        {
            // * Si el usuario existe, devuelve Ok
            return Ok();
        }
        else
        {
            // * Si el usuario no existe, devuelve que no esta Autorizado 
            return Unauthorized();
        }
    }

    [HttpPost("autenticar")]
    public async Task<IActionResult> Autenticar([FromBody] AutorizacionRequest autorizacion)
    {
        // * Llama al servicio para intentar autenticar al usuario y generar un token nuevo
        var resultadoAutorizacion = await _autorizacionService.DevolverToken(autorizacion);
        // * Verifica si la autenticacion fue exitosa
        if (resultadoAutorizacion == null)
            // * Si la autenticacion falla, un BadRequest
            return Unauthorized();

        // * Si la autenticacion fue exitosa, un Ok
        return Ok(resultadoAutorizacion);
    }

    [HttpPost("obtenerRefreshToken")]
    public async Task<IActionResult> ObtenerRefreshToken([FromBody] RefreshTokenRequest request)
    {
        // * Se crea una instancia para leer y validar un token
        var tokenHandler = new JwtSecurityTokenHandler();
        // * Se intenta leer el token JWT
        var tokenExpiradoSupuestamente = tokenHandler.ReadJwtToken(request.TokenExpirado);

        // * Verifica si el token no ha expirado
        if (tokenExpiradoSupuestamente.ValidTo > DateTime.UtcNow)
            // * Si el token no ha expirado, devuelve un BadRequest
            return BadRequest(new AutorizacionResponse { Result = false, Msg = "Token no ha expirado" });

        // * Extrae el ID de usuario del token JWT
        string idUser = tokenExpiradoSupuestamente.Claims.First(x =>
            x.Type == JwtRegisteredClaimNames.NameId
        ).Value.ToString();

        // * Llamar a un servicio para obtener un nuevo token
        var autorizacionResponse = await _autorizacionService.DevolverTokenRefresh(request, int.Parse(idUser));

        // * Construccion de condiciones para procesar las respuestas de la solicitud
        if (autorizacionResponse.Result)
            // * Si la operacion fue exitosa, devuelve un Ok
            return Ok(autorizacionResponse);
        else
            // * Si la operacion fallo, devuelve un BadRequest
            return BadRequest(autorizacionResponse);
    }
}