using System.IO;

namespace KICSITManagementSystem.Oop;

/// <summary>Appends quiz results to the configured text file.</summary>
internal sealed class FileQuizResultWriter : IQuizResultWriter
{
    private readonly string _resultFilePath;

    public FileQuizResultWriter(string resultFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resultFilePath);
        _resultFilePath = resultFilePath;
    }

    /// <inheritdoc />
    public void AppendResultLine(string line) => File.AppendAllText(_resultFilePath, line);
}
