using Spectre.Console;

namespace KICSITManagementSystem.Oop;

/// <summary>
/// Template-method base for role portals: shared loop, hooks for inheritance (override / abstract).
/// Demonstrates inheritance, encapsulation, and polymorphism (virtual welcome line).
/// </summary>
internal abstract class PortalBase : IPortal
{
    private readonly string _username;

    protected PortalBase(string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        _username = username.Trim();
    }

    /// <inheritdoc />
    public string DisplayUsername => _username;

    /// <summary>Template method: fixed control flow; steps vary per subclass.</summary>
    public void Run()
    {
        while (true)
        {
            BeforeMenuIteration();
            DrawRoleHeader();
            WriteWelcomeLine();

            string action = AnsiConsole.Prompt(BuildMenuPrompt());

            if (action == "Logout")
                break;

            UI.ClearScreen();

            if (!ExecuteSelection(action))
                return;

            if (!UI.Confirm(ReturnConfirmPrompt))
                break;
        }
    }

    /// <summary>Called at the start of each menu iteration (override to customize).</summary>
    protected virtual void BeforeMenuIteration() => UI.ClearScreen();

    /// <summary>Role-specific logo / header.</summary>
    protected abstract void DrawRoleHeader();

    /// <summary>Welcome line under the header; override for role-specific markup.</summary>
    protected virtual void WriteWelcomeLine()
    {
        AnsiConsole.MarkupLine(
            $"[bold]Welcome,[/] {GetWelcomeAccentOpening()}{Markup.Escape(DisplayUsername.ToUpperInvariant())}{GetWelcomeAccentClosing()}\n");
    }

    /// <summary>Markup before the escaped username (e.g. color tag).</summary>
    protected abstract string GetWelcomeAccentOpening();

    /// <summary>Closing markup after the username.</summary>
    protected abstract string GetWelcomeAccentClosing();

    protected abstract SelectionPrompt<string> BuildMenuPrompt();

    protected abstract string ReturnConfirmPrompt { get; }

    /// <summary>
    /// Performs the selected action. Returns <c>false</c> to exit the portal without
    /// asking "return to menu?" (e.g. admin navigates to the main menu).
    /// </summary>
    protected abstract bool ExecuteSelection(string action);
}
