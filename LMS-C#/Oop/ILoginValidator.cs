namespace KICSITManagementSystem.Oop;

/// <summary>Strategy abstraction for credential checks (polymorphism: swap implementation).</summary>
internal interface ILoginValidator
{
    /// <summary>Returns whether the password is correct for the given role and username.</summary>
    bool IsValid(string role, string username, string password);
}
