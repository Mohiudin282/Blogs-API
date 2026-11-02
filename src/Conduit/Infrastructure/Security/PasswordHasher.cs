using Microsoft.AspNetCore.Identity;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();
    public Task<byte[]> Hash(string password)
    {
        return Task.FromResult(Convert.FromBase64String(_hasher.HashPassword(this, password)));
    }
    public PasswordVerificationResult Verify(string password, byte[] hash)
    {
        return hash.Length == 0
            ? PasswordVerificationResult.Failed
            : _hasher.VerifyHashedPassword(this, Convert.ToBase64String(hash), password);
    }
    public void Dispose(){}
}