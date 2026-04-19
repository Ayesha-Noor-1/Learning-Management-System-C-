namespace KICSITManagementSystem.Oop;

/// <summary>Small helper type demonstrating method overloading.</summary>
internal static class InputValidation
{
    public static bool IsMeaningful(string? text) =>
        !string.IsNullOrWhiteSpace(text);

    /// <summary>Same intent as <see cref="IsMeaningful"/> but with a max length cap.</summary>
    public static bool IsMeaningful(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return text.Trim().Length <= maxLength;
    }
}
