namespace ZenithHarvest.Tests.Security;

using Xunit;
using ZenithHarvest.Application.Security;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_GeneratesConsistentHashForSamePassword()
    {
        // Arrange
        var password = "SenhaSegura@123";

        // Act
        var hash1 = PasswordHasher.Hash(password);
        var hash2 = PasswordHasher.Hash(password);

        // Assert
        Assert.Equal(hash1, hash2); // SHA256 sem salt é determinístico
    }

    [Fact]
    public void Verify_ReturnsTrueForCorrectPassword()
    {
        // Arrange
        var password = "SenhaSegura@123";
        var hash = PasswordHasher.Hash(password);

        // Act
        var result = PasswordHasher.Verify(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_ReturnsFalseForIncorrectPassword()
    {
        // Arrange
        var password = "SenhaSegura@123";
        var wrongPassword = "OutraSenha@456";
        var hash = PasswordHasher.Hash(password);

        // Act
        var result = PasswordHasher.Verify(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }
}
