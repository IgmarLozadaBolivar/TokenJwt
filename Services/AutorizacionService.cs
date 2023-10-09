using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

    private string GenerarToken(string idUser)
    {
        var Key = _configuration.GetValue<string>("JwtSettings:Key");
        var keyBytes = Encoding.ASCII.GetBytes(Key);

        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, idUser));

        var credencialesToken = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256Signature
        );

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = DateTime.UtcNow.AddMinutes(1),
            SigningCredentials = credencialesToken
        };

        var tokenHeadler = new JwtSecurityTokenHandler();
        var tokenConfig = tokenHeadler.CreateToken(tokenDescriptor);

        string tokenCreado = tokenHeadler.WriteToken(tokenConfig);

        return tokenCreado;
    }

    private string GenerarRefreshToken()
    {
        var byteArray = new byte[64];
        var refreshToken = "";

        using (var mg = RandomNumberGenerator.Create())
        {
            mg.GetBytes(byteArray);
            refreshToken = Convert.ToBase64String(byteArray);
        }
        return refreshToken;
    }

    private async Task<AutorizacionResponse> GuardarRefreshToken(
        int idUser,
        string token,
        string refreshToken
    ){
        var historialRefreshToken = new RefreshToken
        {
            IdUser = idUser,
            Token = token,
            TokenRefresh = refreshToken,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(2),
        };

        await _context.RefreshTokens.AddAsync(historialRefreshToken);
        await _context.SaveChangesAsync();

        return new AutorizacionResponse { Token = token, RefreshToken = refreshToken, Result = true, Msg = "Ok"};
    }

    public async Task<AutorizacionResponse> DevolverToken(AutorizacionRequest autorizacion)
    {
        var usuarioEncontrado = _context.Users.FirstOrDefault(x => 
            x.Username == autorizacion.Username &&
            x.Password == autorizacion.Password
        );

        if(usuarioEncontrado == null) {
            return await Task.FromResult<AutorizacionResponse>(null);
        }

        string tokenCreado = GenerarToken(usuarioEncontrado.Id.ToString());
    
        string refreshTokenCreado = GenerarRefreshToken();

        /* return new AutorizacionResponse() { Token = tokenCreado, Result = true, Msg = "Ok"}; */
        return await GuardarRefreshToken(usuarioEncontrado.Id,tokenCreado, refreshTokenCreado);
    }

    public async Task<AutorizacionResponse> DevolverTokenRefresh(RefreshTokenRequest refreshTokenRequest, int idUser)
    {
        var refreshTokenEncontrado = _context.RefreshTokens.FirstOrDefault(x => 
            x.Token == refreshTokenRequest.TokenExpirado &&
            x.TokenRefresh == refreshTokenRequest.RefreshToken &&
            x.IdUser == idUser);

        if(refreshTokenEncontrado == null)
            return new AutorizacionResponse { Result = false, Msg = "No existe refreshToken"};
    
        var refreshTokenCreado = GenerarRefreshToken();
        var tokenCreado = GenerarToken(idUser.ToString());

        return await GuardarRefreshToken(idUser,tokenCreado,refreshTokenCreado);
    }
}