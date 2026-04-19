namespace KICSITManagementSystem.Oop;

/// <summary>Strategy for persisting quiz attempt summaries (file, DB, etc.).</summary>
internal interface IQuizResultWriter
{
    void AppendResultLine(string line);
}
