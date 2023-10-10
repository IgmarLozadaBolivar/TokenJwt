namespace TokenJwt.Models;

public class RegisterResponse
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public bool Result { get; set; }
    public string Msg { get; set; }
}