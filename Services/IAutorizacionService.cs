using TokenJwt.Models;
namespace TokenJwt.Services;

public interface IAutorizacionService
{
    // * Metodo para registrar el usuario
    Task<RegisterResponse> Registrar(RegisterRequest request);
    // * Metodo para devolver un token
    Task<AutorizacionResponse> DevolverToken(AutorizacionRequest autorizacion);
    // * Metodo para devolver un token refresh
    Task<AutorizacionResponse> DevolverTokenRefresh(RefreshTokenRequest refreshTokenRequest, int idUser);
    Task<bool> VerificarUsuario(AutorizacionRequest autorizacion);
}