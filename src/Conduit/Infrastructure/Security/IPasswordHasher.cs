using Microsoft.AspNetCore.Identity;

public interface IPasswordHasher : IDisposable
{
    public Task<byte[]> Hash(string userPassword);
    public PasswordVerificationResult Verify(string password, byte[] hash);
}