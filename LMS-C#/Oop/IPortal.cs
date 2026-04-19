namespace KICSITManagementSystem.Oop;

/// <summary>
/// Abstraction for any signed-in area (student / teacher / admin). Enables polymorphism
/// when the caller holds <see cref="IPortal"/> and invokes <see cref="Run"/> without
/// knowing the concrete portal type.
/// </summary>
internal interface IPortal
{
    /// <summary>Display name used in welcome text (encapsulated in each portal).</summary>
    string DisplayUsername { get; }

    /// <summary>Runs the portal until the user logs out or exits to the main menu.</summary>
    void Run();
}
