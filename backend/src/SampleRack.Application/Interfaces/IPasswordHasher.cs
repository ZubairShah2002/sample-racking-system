namespace SampleRack.Application.Interfaces;

/// <summary>Hashes and verifies user passwords (BCrypt in the Infrastructure layer).</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
