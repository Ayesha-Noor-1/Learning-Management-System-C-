using MongoDB.Bson;

namespace KICSITManagementSystem.Models;

/// <summary>One saved attempt at the MCQ quiz (stored in MongoDB for reporting).</summary>
internal sealed class QuizAttemptDocument
{
    public ObjectId Id { get; set; }

    /// <summary>Student user who took the quiz (for &quot;my results&quot; filtering).</summary>
    public ObjectId UserId { get; set; }

    public string UsernameNormalized { get; set; } = "";

    /// <summary>Name typed on the result sheet (may differ from login casing).</summary>
    public string SheetName { get; set; } = "";

    public string Roll { get; set; } = "";
    public int Marks { get; set; }
    public int QuestionCount { get; set; }
    public double Percentage { get; set; }
    public long SecondsTaken { get; set; }
    public DateTime TakenUtc { get; set; }
}
