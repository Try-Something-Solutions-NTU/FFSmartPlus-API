namespace Application.Auth;

public class LoginRespDto
{
    public string token { get; set; }
    public DateTime expiration { get; set; }
}