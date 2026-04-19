using KICSITManagementSystem.Oop;
using Xunit;

namespace LMS_C_.Tests;

public class InputValidationTests
{
    [Theory]
    [InlineData("hello", true)]
    [InlineData(" ", false)]
    [InlineData(null, false)]
    public void IsMeaningful_single_arg(string? text, bool expected) =>
        Assert.Equal(expected, InputValidation.IsMeaningful(text));

    [Theory]
    [InlineData("abc", 3, true)]
    [InlineData("abcd", 3, false)]
    [InlineData("", 10, false)]
    public void IsMeaningful_with_max_length(string text, int max, bool expected) =>
        Assert.Equal(expected, InputValidation.IsMeaningful(text, max));
}
