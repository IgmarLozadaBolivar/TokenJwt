using TokenJwt.Models;
namespace TokenJwt.Services;

public interface IAutorizacionService
{
    Task<AutorizacionResponse> DevolverToken(AutorizacionRequest autorizacion);
    Task<AutorizacionResponse> DevolverTokenRefresh(RefreshTokenRequest refreshTokenRequest, int idUser);
}