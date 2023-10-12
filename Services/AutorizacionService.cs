using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TokenJwt.Entities;
using TokenJwt.Models;
namespace TokenJwt.Services;

public class AutorizacionService : IAutorizacionService
{
    private readonly DbAppContext _context;
    private readonly IConfiguration _configuration;

    public AutorizacionService(DbAppContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<RegisterResponse> Registrar(RegisterRequest request)
    {
        // * Verifica si el usuario ya existe en la base de datos
        var usuarioExistente = _context.Users.FirstOrDefault(x => x.Username == request.Username);
        if (usuarioExistente != null)
        {
            // * Si existe devuelve al usuario con un mensaje
            return new RegisterResponse { Result = false, Msg = "El nombre de usuario ya esta existe!" };
        }

        // * Crea un nuevo usuario u objecto de el
        var nuevoUsuario = new User
        {
            Username = request.Username,
            Password = request.Password
        };

        // * Agrega al nuevo usuario a la base de datos
        _context.Users.Add(nuevoUsuario);

        // * Guarda cambios en la base de datos
        await _context.SaveChangesAsync();

        // * Genera un token de autenticacion para el nuevo usuario
        var tokenCreado = GenerarToken(nuevoUsuario.Id.ToString());

        // * Genera un nuevo refresh token para el nuevo usuario
        var refreshTokenCreado = GenerarRefreshToken();

        // * Guarda el refresh token en la Db
        await GuardarRefreshToken(nuevoUsuario.Id, tokenCreado, refreshTokenCreado);

        // * Devueleve una nueva respuesta con el token y el refresh token
        return new RegisterResponse { Token = tokenCreado, RefreshToken = refreshTokenCreado, Result = true, Msg = "Registro exitoso!" };
    }

    public async Task<bool> VerificarUsuario(AutorizacionRequest autorizacion)
    {
        // * Verificamos si el usuario existe en la Db
        var usuarioEncontrado = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Username == autorizacion.Username &&
                x.Password == autorizacion.Password
            );

        // * Si el Usuario Encontrado no es nulo, significa que el usuario existe
        return usuarioEncontrado != null;
    }

    private string GenerarToken(string idUser)
    {
        // * Se obtiene la clave secreta para firmar el token desde la configuracion
        var Key = _configuration.GetValue<string>("JwtSettings:Key");
        // * Se convierte la clave secreta en bytes
        var keyBytes = Encoding.ASCII.GetBytes(Key);

        // * Se configuran las credenciales para firmar el token
        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, idUser));

        var credencialesToken = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),    // * Clave secreta para firmar el token
            SecurityAlgorithms.HmacSha256Signature    // * Algoritmo de firma (HMAC-SHA256)
        );

        // * Se configura un descriptor del token que incluye informacion 
        // * como las reclamaciones, la fecha de vencimiento y las credenciales de firma
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            
            Subject = claims,    // * Reclamaciones del token
            Expires = DateTime.UtcNow.AddMinutes(1),    // * Fecha de vencimiento
            SigningCredentials = credencialesToken    // * Credenciales de firma
        };

        // * Crea un manejador de tokens JWT
        var tokenHeadler = new JwtSecurityTokenHandler();
        // * Crea el token JWT en base al descriptor
        var tokenConfig = tokenHeadler.CreateToken(tokenDescriptor);

        // * Se genera una cadena a partir del token JWT
        string tokenCreado = tokenHeadler.WriteToken(tokenConfig);

        // * Retorna el token JWT completamente firmado
        return tokenCreado;
    }

    private string GenerarRefreshToken()
    {
        // * Crea un arreglo de bytes de longitud 64
        var byteArray = new byte[64];
        var refreshToken = "";

        // * Crea una instancia de RandomNumberGenerator
        using (var mg = RandomNumberGenerator.Create())
        {
            // * Se llena el arreglo de bytes
            mg.GetBytes(byteArray);
            // * Se convierte el arreglo de bytes en una cadena Base64
            refreshToken = Convert.ToBase64String(byteArray);
        }
        // * Se retorna el token aleatorio
        return refreshToken;
    }

    private async Task<AutorizacionResponse> GuardarRefreshToken(
        int idUser,
        string token,
        string refreshToken
    )
    {
        // * Creamos un objeto RefreshToken
        var descriptorRefreshToken = new RefreshToken
        {
            IdUser = idUser,    // * ID del usuario asociado al token
            Token = token,  // * Token asociado al refresh token
            TokenRefresh = refreshToken,    // * El refresh token aleatorio
            Created = DateTime.UtcNow,  // * Marca de tiempo actual en formato UTC
            Expires = DateTime.UtcNow.AddMinutes(2),    // * Fecha de vencimiento de 2 minutos
        };

        // * Se agrega el objeto RefreshToken en la Db
        await _context.RefreshTokens.AddAsync(descriptorRefreshToken);
        // * Se salvan los cambios en la Db
        await _context.SaveChangesAsync();

        // * Me retorna una respuesta exitosa con el token y el refresh token
        return new AutorizacionResponse { Token = token, RefreshToken = refreshToken, Result = true, Msg = "Todo bien!" };
    }

    public async Task<AutorizacionResponse> DevolverToken(AutorizacionRequest autorizacion)
    {
        // * Busca al usuario por medio de Nombre y contraseña
        var usuarioEncontrado = _context.Users.FirstOrDefault(x =>
            x.Username == autorizacion.Username &&
            x.Password == autorizacion.Password
        );

        // * Si el usuario no se encuentra, retorna una respuesta nula
        if (usuarioEncontrado == null)
        {
            return await Task.FromResult<AutorizacionResponse>(null);
        }

        // * Genera un nuevo token JWT utilizando el ID del usuario
        string tokenCreado = GenerarToken(usuarioEncontrado.Id.ToString());

        // * Genera un nuevo token refresh
        string refreshTokenCreado = GenerarRefreshToken();

        // * Llama a la funcion de GuardarRefreshToken y guarda el token y refresh token
        /* return new AutorizacionResponse() { Token = tokenCreado, Result = true, Msg = "Ok"}; */
        return await GuardarRefreshToken(usuarioEncontrado.Id, tokenCreado, refreshTokenCreado);
    }

    public async Task<AutorizacionResponse> DevolverTokenRefresh(RefreshTokenRequest refreshTokenRequest, int idUser)
    {
        // * Busca un token refresh que exista en la Db
        var refreshTokenEncontrado = _context.RefreshTokens.FirstOrDefault(x =>
            x.Token == refreshTokenRequest.TokenExpirado &&
            x.TokenRefresh == refreshTokenRequest.RefreshToken &&
            x.IdUser == idUser);

        // * Si no se encuentra un token valido, retorna un null y un mensaje
        if (refreshTokenEncontrado == null)
            return new AutorizacionResponse { Result = false, Msg = "No existe refreshToken" };

        // * Genera un nuevo token refresh
        var refreshTokenCreado = GenerarRefreshToken();
        //* Genera un nuevo token
        var tokenCreado = GenerarToken(idUser.ToString());

        // * Llama a la funcion GuardarRefreshToken y actualiza el token y refresh token
        return await GuardarRefreshToken(idUser, tokenCreado, refreshTokenCreado);
    }
}