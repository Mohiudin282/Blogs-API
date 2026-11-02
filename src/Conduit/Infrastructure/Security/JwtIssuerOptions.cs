using Microsoft.IdentityModel.Tokens;

public class JwtIssuerOptions
{
    public const string Schemes = "Bearer";
    public string? Issuer {get; set;}
    public string? Subject {get; set;}
    public string? Audience {get; set;}
    public DateTime NotBefore => DateTime.UtcNow;
    public DateTime IssuedAt => DateTime.UtcNow;
    public TimeSpan ValidFor {get; set;} = TimeSpan.FromMinutes(5);
    public DateTime Expiration => IssuedAt.Add(ValidFor);
    public Func<string> JtiGenerator => () => Guid.NewGuid().ToString();
    public SigningCredentials? SigningCredentials {get; set;}
}