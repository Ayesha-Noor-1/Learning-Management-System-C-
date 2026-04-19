using KICSITManagementSystem.Data;
using Xunit;

namespace LMS_C_.Tests;

public class UserRepositoryNormalizeTests
{
    [Theory]
    [InlineData("  Ali  ", "ali")]
    [InlineData("USER", "user")]
    public void Normalize_trims_and_lowercases(string input, string expected) =>
        Assert.Equal(expected, UserRepository.Normalize(input));
}
